using files = org.pdfclown.files;
using streams = org.pdfclown.bytes;
using org.pdfclown.documents.interaction.annotations;
using org.pdfclown.documents.interaction.forms;
using org.pdfclown.objects;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace org.pdfclown.tools {

    public interface ISignatureGenerator {

        // Allocates space for a signature of the specifed size and with the specified name.
        // Returns the bytes which the resulting signature is to cover.
        byte[] Prepare(int spaceInBytes, string name, IDictionary<PdfName, PdfDirectObject> entries);

        // Applies the signature and returns the resulting signed PDF document.
        byte[] Apply(byte[] signature);
    }

    public interface ISignatureHelper {

        // The PDF
        byte[] PdfBytes { get; }

        // Low level access to the PDF
        files.File File { get; }

        // The existing signatures in the order in which they were applied.
        IList<ISignature> Signatures { get; }

        // Disposes the object
        void Dispose();
    }

    public static class SignatureManager {

        #region SignatureGenerator

        private class SignatureGenerator : ISignatureGenerator {

            private byte[] originalPdf;
            private byte[] signedPdf;
            private PdfSignatureDictionary signatureDictionary;

            public SignatureGenerator(byte[] pdf) {

                originalPdf = pdf;
            }

            public byte[] Prepare(int spaceInBytes, string name, IDictionary<PdfName, PdfDirectObject> entries) {

                signatureDictionary = new PdfSignatureDictionary(spaceInBytes, entries);

                using (files.File file = new files.File(new streams.Stream(new MemoryStream(originalPdf)))) {

                    // Add signature dictionary
                    file.IndirectObjects.Add(signatureDictionary);

                    // Add signature form field
                    SignatureField sigField = new SignatureField(name ?? Guid.NewGuid().ToString(), new Widget(file.Document.Pages.First()));
                    sigField.Value = signatureDictionary.Reference;
                    file.Document.Form.Fields.Add(sigField);
                    file.Document.Form.SigFlags = Form.SigFlagsEnum.AppendOnly | Form.SigFlagsEnum.SignatureExist;

                    // Serialize the pdf
                    MemoryStream ms = new MemoryStream();
                    streams.IOutputStream s = new streams.Stream(ms);
                    file.Save(s, files.SerializationModeEnum.Incremental);

                    // Update the byte range
                    PdfArray byteRange = new PdfArray(
                        new PdfInteger(0),
                        new PdfInteger(signatureDictionary.ContentsStart - 1),
                        new PdfInteger(signatureDictionary.ContentsStart + signatureDictionary.SignatureSize * 2 + 1),
                        new PdfInteger((int)ms.Length - (signatureDictionary.ContentsStart + signatureDictionary.SignatureSize * 2 + 1))
                    );
                    streams.IOutputStream os = new streams.Stream(ms);
                    os.Position = signatureDictionary.ByteRangeStart;
                    byteRange.WriteTo(os, file);
                    signedPdf = ms.ToArray();

                    // Assemble the actual bytes over which the signature is generated
                    MemoryStream targetStream = new MemoryStream();
                    targetStream.Write(signedPdf, 0, signatureDictionary.ContentsStart - 1);
                    targetStream.Write(signedPdf, signatureDictionary.ContentsStart + signatureDictionary.SignatureSize * 2 + 1, (int)ms.Length - (signatureDictionary.ContentsStart + signatureDictionary.SignatureSize * 2 + 1));
                    targetStream.Position = 0;

                    return targetStream.ToArray();
                }
            }

            public byte[] Apply(byte[] signature) {

                byte[] bytes = Encoding.ASCII.GetBytes((string)new PdfString(signature, PdfString.SerializationModeEnum.Hex).Value);
                Array.Copy(bytes, 0, signedPdf, signatureDictionary.ContentsStart, bytes.Length);

                return signedPdf;
            }
        }

        #endregion

        #region SignatureHelper

        public class SignatureHelper : ISignatureHelper, IDisposable {

            private bool isDisposed;
            private files.File file;

            public byte[] PdfBytes { get; private set; }
            public IList<ISignature> Signatures { get; private set; }

            public SignatureHelper(byte[] pdf) {

                Signatures = new List<ISignature>();
                PdfBytes = pdf;
                file = new files.File(new streams.Stream(new MemoryStream(PdfBytes)));

                foreach (ISignature signature in File.Document.Form.Signatures) {
                    Signatures.Add(signature);
                }
            }

            public files.File File {
                get {
                    if (!isDisposed) {
                        return file;
                    }
                    throw new ObjectDisposedException("The PDF have been disposed!");
                }
            }

            public void Dispose() {

                Dispose(true);
            }

            protected virtual void Dispose(bool disposing) {

                if (!isDisposed) {
                    if (disposing) {
                        if (File != null) {
                            File.Dispose();
                        }
                    }

                    file = null;
                    PdfBytes = null;
                    isDisposed = true;
                }
            }
        }

        #endregion

        public static ISignatureGenerator CreateGenerator(byte[] pdf) {

            return new SignatureGenerator(pdf);
        }

        public static ISignatureHelper CreateHelper(byte[] pdf) {

            return new SignatureHelper(pdf);
        }
    }
}
