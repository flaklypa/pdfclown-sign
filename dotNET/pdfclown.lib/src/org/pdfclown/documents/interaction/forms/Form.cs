/*
  Copyright 2008-2012 Stefano Chizzolini. http://www.pdfclown.org

  Contributors:
    * Stefano Chizzolini (original code developer, http://www.stefanochizzolini.it)

  This file should be part of the source code distribution of "PDF Clown library" (the
  Program): see the accompanying README files for more info.

  This Program is free software; you can redistribute it and/or modify it under the terms
  of the GNU Lesser General Public License as published by the Free Software Foundation;
  either version 3 of the License, or (at your option) any later version.

  This Program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY,
  either expressed or implied; without even the implied warranty of MERCHANTABILITY or
  FITNESS FOR A PARTICULAR PURPOSE. See the License for more details.

  You should have received a copy of the GNU Lesser General Public License along with this
  Program (see README files); if not, go to the GNU website (http://www.gnu.org/licenses/).

  Redistribution and use, with or without modification, are permitted provided that such
  redistributions retain the above copyright notice, license and disclaimer, along with
  this list of conditions.
*/

using org.pdfclown.bytes;
using org.pdfclown.documents;
using org.pdfclown.documents.contents;
using org.pdfclown.documents.interaction.annotations;
using org.pdfclown.objects;

using System;
using System.Collections.Generic;
using System.Linq;

namespace org.pdfclown.documents.interaction.forms
{
  /**
    <summary>Interactive form (AcroForm) [PDF:1.6:8.6.1].</summary>
  */
  [PDF(VersionEnum.PDF12)]
  public sealed class Form
    : PdfObjectWrapper<PdfDictionary>
  {
    #region static
    #region interface
    #region public
    public static Form Wrap(
      PdfDirectObject baseObject
      )
    {return baseObject != null ? new Form(baseObject) : null;}
    #endregion
    #endregion
    #endregion

    #region dynamic
    #region constructors
    public Form(
      Document context
      ) : base(
        context,
        new PdfDictionary(
          new PdfName[]
          {PdfName.Fields},
          new PdfDirectObject[]
          {new PdfArray()}
          )
        )
    {}

    private Form(
      PdfDirectObject baseObject
      ) : base(baseObject)
    {}
    #endregion

    #region interface
    #region public
    /**
      <summary>Gets/Sets the fields collection.</summary>
    */
    public Fields Fields
    {
      get
      {return new Fields(BaseDataObject.Get<PdfArray>(PdfName.Fields));}
      set
      {BaseDataObject[PdfName.Fields] = value.BaseObject;}
    }

    /**
      <summary>Gets/Sets the default resources used by fields.</summary>
    */
    public Resources Resources
    {
      get
      {return Resources.Wrap(BaseDataObject.Get<PdfDictionary>(PdfName.DR));}
      set
      {BaseDataObject[PdfName.DR] = value.BaseObject;}
    }

    /**
      <summary>Returns true if this document has signatures.</summary>
    */
    public bool HasSignatures {
        get { return Signatures.Count > 0; }
    }

    /**
      <summary>List the signatures in the order in  which they were added.</summary>
    */
    public IList<ISignature> Signatures {
        get { return Signature.RetrieveAllSignatureFields(Fields).OrderBy(s => s.SignedRevisionSize).ToList(); }
    }
    #endregion
    #region private
    private class Signature : ISignature {

        private readonly int[] FirstByteRange;
        private readonly int[] SecondByteRange;

        public Signature(SignatureField signatureField) {

            PdfIndirectObject obj = signatureField.File.IndirectObjects[(signatureField.Value as PdfReference).ObjectNumber];
            PdfSignatureDictionary SignatureDictionary = new PdfSignatureDictionary((PdfDictionary)obj.Resolve());
            if (SignatureDictionary[PdfSignatureDictionary.Filter] != null) {
                Filter = SignatureDictionary[PdfSignatureDictionary.Filter].ToString();
            }
            if (SignatureDictionary[PdfSignatureDictionary.SubFilter] != null) {
                SubFilter = SignatureDictionary[PdfSignatureDictionary.SubFilter].ToString();
            }
            PdfArray byteRange = SignatureDictionary[PdfSignatureDictionary.ByteRange] as PdfArray;
            FirstByteRange = new int[] { ((PdfInteger)(byteRange)[0]).IntValue, ((PdfInteger)(byteRange)[1]).IntValue };
            SecondByteRange = new int[] { ((PdfInteger)(byteRange)[2]).IntValue, ((PdfInteger)(byteRange)[3]).IntValue };
            Name = signatureField.Name;
        }

        public string Name { get; private set; }

        public string Filter { get; private set; }

        public string SubFilter { get; private set; }

        public int[] SignatureRange {
            get { return new int[] { FirstByteRange[1] + 1, SecondByteRange[0] - FirstByteRange[1] - 2 }; }
        }

        public int[] SignedBytesRange {
            get { return new int[] { FirstByteRange[0], FirstByteRange[1], SecondByteRange[0], SecondByteRange[1] }; }
        }

        public int[] SignedRevisionRange {
            get { return new int[] { FirstByteRange[0], SecondByteRange.Sum() }; }
        }

        public int SignedRevisionSize {
            get { return SecondByteRange.Sum(); }
        }

        public static List<ISignature> RetrieveAllSignatureFields(Fields fields) {
            return (from field in fields.Values
                    where field.BaseDataObject[PdfName.FT].Equals(PdfName.Sig)
                    select new Signature(field as SignatureField) as ISignature).ToList();
        }
    }
    #endregion
    #endregion
    #endregion
  }
}