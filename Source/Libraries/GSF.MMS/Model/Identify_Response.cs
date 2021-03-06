//
// This file was generated by the BinaryNotes compiler.
// See http://bnotes.sourceforge.net 
// Any modifications to this file will be lost upon recompilation of the source ASN.1. 
//

using System.Runtime.CompilerServices;
using System.Collections.Generic;
using GSF.ASN1;
using GSF.ASN1.Attributes;
using GSF.ASN1.Coders;
using GSF.ASN1.Types;

namespace GSF.MMS.Model
{
    
    [ASN1PreparedElement]
    [ASN1Sequence(Name = "Identify_Response", IsSet = false)]
    public class Identify_Response : IASN1PreparedElement
    {
        private static readonly IASN1PreparedElementData preparedData = CoderFactory.getInstance().newPreparedElementData(typeof(Identify_Response));
        private ICollection<ObjectIdentifier> listOfAbstractSyntaxes_;

        private bool listOfAbstractSyntaxes_present;
        private MMSString modelName_;
        private MMSString revision_;
        private MMSString vendorName_;

        [ASN1Element(Name = "vendorName", IsOptional = false, HasTag = true, Tag = 0, HasDefaultValue = false)]
        public MMSString VendorName
        {
            get
            {
                return vendorName_;
            }
            set
            {
                vendorName_ = value;
            }
        }

        [ASN1Element(Name = "modelName", IsOptional = false, HasTag = true, Tag = 1, HasDefaultValue = false)]
        public MMSString ModelName
        {
            get
            {
                return modelName_;
            }
            set
            {
                modelName_ = value;
            }
        }

        [ASN1Element(Name = "revision", IsOptional = false, HasTag = true, Tag = 2, HasDefaultValue = false)]
        public MMSString Revision
        {
            get
            {
                return revision_;
            }
            set
            {
                revision_ = value;
            }
        }

        [ASN1ObjectIdentifier(Name = "listOfAbstractSyntaxes")]
        //[ASN1SequenceOf(Name = "listOfAbstractSyntaxes", IsSetOf = false)]
        [ASN1Element(Name = "listOfAbstractSyntaxes", IsOptional = true, HasTag = true, Tag = 3, HasDefaultValue = false)]
        public ICollection<ObjectIdentifier> ListOfAbstractSyntaxes
        {
            get
            {
                return listOfAbstractSyntaxes_;
            }
            set
            {
                listOfAbstractSyntaxes_ = value;
                listOfAbstractSyntaxes_present = true;
            }
        }

        public void initWithDefaults()
        {
        }

        public IASN1PreparedElementData PreparedData
        {
            get
            {
                return preparedData;
            }
        }

        public bool isListOfAbstractSyntaxesPresent()
        {
            return listOfAbstractSyntaxes_present;
        }
    }
}