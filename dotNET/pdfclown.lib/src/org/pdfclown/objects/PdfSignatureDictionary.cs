using System;
using System.Collections.Generic;
using System.Linq;
using text = System.Text;

using org.pdfclown.bytes;
using org.pdfclown.files;
using org.pdfclown.tokens;

namespace org.pdfclown.objects {

    public sealed class PdfSignatureDictionary : PdfDirectObject, IDictionary<PdfName, PdfDirectObject> {

        private static readonly byte[] BeginDictionaryChunk = Encoding.Pdf.Encode(Keyword.BeginDictionary);
        private static readonly byte[] EndDictionaryChunk = Encoding.Pdf.Encode(Keyword.EndDictionary);
        public static readonly PdfName Filter = new PdfName("Filter");
        public static readonly PdfName SubFilter = new PdfName("SubFilter");
        public static readonly PdfName ByteRange = new PdfName("ByteRange");
        public static readonly PdfName Contents = new PdfName("Contents");

        private readonly PdfDictionary BackingDictionary;

        public int SignatureSize { get; private set; }
        public int ContentsStart { get; private set; }
        public int ByteRangeStart { get; private set; }

        public PdfSignatureDictionary(PdfDictionary dict) {

            BackingDictionary = dict;
        }

        public PdfSignatureDictionary(int signatureSize, IDictionary<PdfName, PdfDirectObject> entries) {

            BackingDictionary = new PdfDictionary(entries);

            SignatureSize = signatureSize;

            BackingDictionary.Add(PdfName.FT, PdfName.Sig);
            BackingDictionary.Add(ByteRange, new PdfString(new string(' ', 100), PdfString.SerializationModeEnum.Verbatim));
            BackingDictionary.Add(Contents, new PdfString(new String('0', signatureSize * 2), PdfString.SerializationModeEnum.Hex));
        }

        public override void WriteTo(IOutputStream stream, File context) {

            // Begin.
            stream.Write(BeginDictionaryChunk);

            // Entries.
            foreach (KeyValuePair<PdfName, PdfDirectObject> entry in BackingDictionary.entries) {

                PdfDirectObject value = entry.Value;
                if (value != null && value.Virtual)
                    continue;

                if (entry.Key == Contents) { 
                    ContentsStart = (int)stream.Position + 11; 
                } else if (entry.Key == ByteRange) { 
                    ByteRangeStart = (int)stream.Position + 11; 
                }

                // ...key.
                entry.Key.WriteTo(stream, context); 
                stream.Write(Chunk.Space);

                // ...value.
                PdfDirectObject.WriteTo(stream, context, value); 
                stream.Write(Chunk.Space);
            }

            // End.
            stream.Write(EndDictionaryChunk);
        }

        #region PdfDirectObject implementation refers to the backing dictionary

        public override PdfObject Parent {
            get { return BackingDictionary.Parent; }
            internal set { BackingDictionary.Parent = value; }
        }

        public override bool Updateable {
            get { return BackingDictionary.Updateable; }
            set { BackingDictionary.Updateable = value; }
        }

        public override bool Updated {
            get { return BackingDictionary.Updated; }
            protected internal set { BackingDictionary.Updated = value; }
        }

        protected internal override bool Virtual {
            get { return BackingDictionary.Virtual; }
            set { BackingDictionary.Virtual = value; }
        }

        public override int CompareTo(PdfDirectObject obj) {
            return BackingDictionary.CompareTo(obj);
        }

        public override PdfObject Swap(PdfObject other) {
            return BackingDictionary.Swap(other);
        }

        public override PdfObject Accept(IVisitor visitor, object data) {
            return BackingDictionary.Accept(visitor, data);
        }

        #endregion

        #region IDictionary implementation refers to the backing dictionary

        public void Add(PdfName key, PdfDirectObject value) {
            BackingDictionary.Add(key, value);
        }

        public bool ContainsKey(PdfName key) {
            return BackingDictionary.ContainsKey(key);
        }

        public ICollection<PdfName> Keys {
            get { return BackingDictionary.Keys; }
        }

        public bool Remove(PdfName key) {
            return BackingDictionary.Remove(key);
        }

        public bool TryGetValue(PdfName key, out PdfDirectObject value) {
            return BackingDictionary.TryGetValue(key, out value);
        }

        public ICollection<PdfDirectObject> Values {
            get { return BackingDictionary.Values; }
        }

        public PdfDirectObject this[PdfName key] {
            get { return BackingDictionary[key]; }
            set { BackingDictionary[key] = value; }
        }

        public void Add(KeyValuePair<PdfName, PdfDirectObject> item) {
            BackingDictionary.Add(item.Key, item.Value);
        }

        public void Clear() {
            BackingDictionary.Clear();
        }

        public bool Contains(KeyValuePair<PdfName, PdfDirectObject> item) {
            return BackingDictionary.Contains(item);
        }

        public void CopyTo(KeyValuePair<PdfName, PdfDirectObject>[] array, int arrayIndex) {
            BackingDictionary.CopyTo(array, arrayIndex);
        }

        public int Count {
            get { return BackingDictionary.Count; }
        }

        public bool IsReadOnly {
            get { return BackingDictionary.IsReadOnly; }
        }

        public bool Remove(KeyValuePair<PdfName, PdfDirectObject> item) {
            return BackingDictionary.Remove(item);
        }

        public IEnumerator<KeyValuePair<PdfName, PdfDirectObject>> GetEnumerator() {
            return BackingDictionary.entries.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return BackingDictionary.entries.GetEnumerator();
        }

        #endregion
    }
}
