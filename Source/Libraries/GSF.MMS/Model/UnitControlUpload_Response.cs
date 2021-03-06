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

namespace GSF.MMS.Model
{
    
    [ASN1PreparedElement]
    [ASN1Sequence(Name = "UnitControlUpload_Response", IsSet = false)]
    public class UnitControlUpload_Response : IASN1PreparedElement
    {
        private static readonly IASN1PreparedElementData preparedData = CoderFactory.getInstance().newPreparedElementData(typeof(UnitControlUpload_Response));
        private ICollection<ControlElement> controlElements_;


        private NextElementChoiceType nextElement_;

        private bool nextElement_present;

        [ASN1SequenceOf(Name = "controlElements", IsSetOf = false)]
        [ASN1Element(Name = "controlElements", IsOptional = false, HasTag = true, Tag = 0, HasDefaultValue = false)]
        public ICollection<ControlElement> ControlElements
        {
            get
            {
                return controlElements_;
            }
            set
            {
                controlElements_ = value;
            }
        }


        [ASN1Element(Name = "nextElement", IsOptional = true, HasTag = false, HasDefaultValue = false)]
        public NextElementChoiceType NextElement
        {
            get
            {
                return nextElement_;
            }
            set
            {
                nextElement_ = value;
                nextElement_present = true;
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

        public bool isNextElementPresent()
        {
            return nextElement_present;
        }

        [ASN1PreparedElement]
        [ASN1Choice(Name = "nextElement")]
        public class NextElementChoiceType : IASN1PreparedElement
        {
            private static IASN1PreparedElementData preparedData = CoderFactory.getInstance().newPreparedElementData(typeof(NextElementChoiceType));
            private Identifier domain_;
            private bool domain_selected;
            private Identifier programInvocation_;
            private bool programInvocation_selected;


            private long ulsmID_;
            private bool ulsmID_selected;

            [ASN1Element(Name = "domain", IsOptional = false, HasTag = true, Tag = 1, HasDefaultValue = false)]
            public Identifier Domain
            {
                get
                {
                    return domain_;
                }
                set
                {
                    selectDomain(value);
                }
            }


            [ASN1Integer(Name = "")]
            [ASN1Element(Name = "ulsmID", IsOptional = false, HasTag = true, Tag = 2, HasDefaultValue = false)]
            public long UlsmID
            {
                get
                {
                    return ulsmID_;
                }
                set
                {
                    selectUlsmID(value);
                }
            }


            [ASN1Element(Name = "programInvocation", IsOptional = false, HasTag = true, Tag = 3, HasDefaultValue = false)]
            public Identifier ProgramInvocation
            {
                get
                {
                    return programInvocation_;
                }
                set
                {
                    selectProgramInvocation(value);
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


            public bool isDomainSelected()
            {
                return domain_selected;
            }


            public void selectDomain(Identifier val)
            {
                domain_ = val;
                domain_selected = true;


                ulsmID_selected = false;

                programInvocation_selected = false;
            }


            public bool isUlsmIDSelected()
            {
                return ulsmID_selected;
            }


            public void selectUlsmID(long val)
            {
                ulsmID_ = val;
                ulsmID_selected = true;


                domain_selected = false;

                programInvocation_selected = false;
            }


            public bool isProgramInvocationSelected()
            {
                return programInvocation_selected;
            }


            public void selectProgramInvocation(Identifier val)
            {
                programInvocation_ = val;
                programInvocation_selected = true;


                domain_selected = false;

                ulsmID_selected = false;
            }
        }
    }
}