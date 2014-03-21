using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace org.pdfclown.documents.interaction.forms {

    public interface ISignature {

        /// <summary>
        /// The partial signature field name (see PDF 1.7, 8.6.2). 
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The name of the preferred signature handler to use when
        /// validating this signature (see PDF 1.7, 8.7). 
        /// </summary>
        string Filter { get; }

        /// <summary>
        /// A name that describes the encoding of the signature value and key 
        /// information in the signature dictionary (see PDF 1.7, 8.7).
        /// </summary>
        string SubFilter { get; }

        /// <summary>
        /// The byte range covering the signature.
        /// Note that the signature probably take up only part of this space.
        /// </summary>
        int[] SignatureRange { get; }

        /// <summary>
        /// The byte ranges covering the data over which the signature is calculated.
        /// </summary>
        int[] SignedBytesRange { get; }

        /// <summary>
        /// The byte range covering the document after this signature
        /// was applied i.e. the signed document revision.
        /// </summary>
        int[] SignedRevisionRange { get; }

        /// <summary>
        /// The size in bytes of this revision of the document.
        /// </summary>
        int SignedRevisionSize { get; }

        /// <summary>
        /// The allocated space for this signature (note that since it's hex encoded,
        /// only half of this is available for the actual signature). 
        /// </summary>
        int MaxSignatureSize { get; }
    }
}
