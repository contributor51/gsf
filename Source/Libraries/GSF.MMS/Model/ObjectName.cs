//
// This file was generated by the BinaryNotes compiler.
// See http://bnotes.sourceforge.net 
// Any modifications to this file will be lost upon recompilation of the source ASN.1. 
//

using System.Runtime.CompilerServices;
using GSF.ASN1;
using GSF.ASN1.Attributes;
using GSF.ASN1.Coders;

namespace GSF.MMS.Model
{
    
    [ASN1PreparedElement]
    [ASN1Choice(Name = "ObjectName")]
    public class ObjectName : IASN1PreparedElement
    {
        private static readonly IASN1PreparedElementData preparedData = CoderFactory.getInstance().newPreparedElementData(typeof(ObjectName));
        private Identifier aa_specific_;
        private bool aa_specific_selected;
        private Domain_specificSequenceType domain_specific_;
        private bool domain_specific_selected;
        private Identifier vmd_specific_;
        private bool vmd_specific_selected;


        [ASN1Element(Name = "vmd-specific", IsOptional = false, HasTag = true, Tag = 0, HasDefaultValue = false)]
        public Identifier Vmd_specific
        {
            get
            {
                return vmd_specific_;
            }
            set
            {
                selectVmd_specific(value);
            }
        }


        [ASN1Element(Name = "domain-specific", IsOptional = false, HasTag = true, Tag = 1, HasDefaultValue = false)]
        public Domain_specificSequenceType Domain_specific
        {
            get
            {
                return domain_specific_;
            }
            set
            {
                selectDomain_specific(value);
            }
        }


        [ASN1Element(Name = "aa-specific", IsOptional = false, HasTag = true, Tag = 2, HasDefaultValue = false)]
        public Identifier Aa_specific
        {
            get
            {
                return aa_specific_;
            }
            set
            {
                selectAa_specific(value);
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


        public bool isVmd_specificSelected()
        {
            return vmd_specific_selected;
        }


        public void selectVmd_specific(Identifier val)
        {
            vmd_specific_ = val;
            vmd_specific_selected = true;


            domain_specific_selected = false;

            aa_specific_selected = false;
        }


        public bool isDomain_specificSelected()
        {
            return domain_specific_selected;
        }


        public void selectDomain_specific(Domain_specificSequenceType val)
        {
            domain_specific_ = val;
            domain_specific_selected = true;


            vmd_specific_selected = false;

            aa_specific_selected = false;
        }


        public bool isAa_specificSelected()
        {
            return aa_specific_selected;
        }


        public void selectAa_specific(Identifier val)
        {
            aa_specific_ = val;
            aa_specific_selected = true;

            vmd_specific_selected = false;

            domain_specific_selected = false;
        }

        [ASN1PreparedElement]
        [ASN1Sequence(Name = "domain-specific", IsSet = false)]
        public class Domain_specificSequenceType : IASN1PreparedElement
        {
            private static IASN1PreparedElementData preparedData = CoderFactory.getInstance().newPreparedElementData(typeof(Domain_specificSequenceType));
            private Identifier domainID_;


            private Identifier itemID_;

            [ASN1Element(Name = "domainID", IsOptional = false, HasTag = false, HasDefaultValue = false)]
            public Identifier DomainID
            {
                get
                {
                    return domainID_;
                }
                set
                {
                    domainID_ = value;
                }
            }

            [ASN1Element(Name = "itemID", IsOptional = false, HasTag = false, HasDefaultValue = false)]
            public Identifier ItemID
            {
                get
                {
                    return itemID_;
                }
                set
                {
                    itemID_ = value;
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
        }
    }
}