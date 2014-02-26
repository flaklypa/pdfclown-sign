using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace org.pdfclown.documents.interaction.forms {

    public interface ISignature {

        string Name { get; }
        string Filter { get; }
        string SubFilter { get; }
        int[] SignatureRange { get; }
        int[] SignedBytesRange { get; }
        int[] SignedRevisionRange { get; }
        int SignedRevisionSize { get; }
    }
}
