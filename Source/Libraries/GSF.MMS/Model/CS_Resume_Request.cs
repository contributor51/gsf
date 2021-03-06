//
// This file was generated by the BinaryNotes compiler.
// See http://bnotes.sourceforge.net 
// Any modifications to this file will be lost upon recompilation of the source ASN.1. 
//

using System.Runtime.CompilerServices;
using GSF.ASN1;
using GSF.ASN1.Attributes;
using GSF.ASN1.Coders;
using GSF.ASN1.Types;

namespace GSF.MMS.Model
{
    
    [ASN1PreparedElement]
    [ASN1BoxedType(Name = "CS_Resume_Request")]
    public class CS_Resume_Request : IASN1PreparedElement
    {
        private static readonly IASN1PreparedElementData preparedData = CoderFactory.getInstance().newPreparedElementData(typeof(CS_Resume_Request));
        private CS_Resume_RequestChoiceType val;


        [ASN1Element(Name = "CS-Resume-Request", IsOptional = false, HasTag = true, Tag = 0, HasDefaultValue = false)]
        public CS_Resume_RequestChoiceType Value
        {
            get
            {
                return val;
            }

            set
            {
                val = value;
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

        [ASN1PreparedElement]
        [ASN1Choice(Name = "CS-Resume-Request")]
        public class CS_Resume_RequestChoiceType : IASN1PreparedElement
        {
            private static IASN1PreparedElementData preparedData = CoderFactory.getInstance().newPreparedElementData(typeof(CS_Resume_RequestChoiceType));
            private ControllingSequenceType controlling_;
            private bool controlling_selected;
            private NullObject normal_;
            private bool normal_selected;


            [ASN1Null(Name = "normal")]
            [ASN1Element(Name = "normal", IsOptional = false, HasTag = false, HasDefaultValue = false)]
            public NullObject Normal
            {
                get
                {
                    return normal_;
                }
                set
                {
                    selectNormal(value);
                }
            }


            [ASN1Element(Name = "controlling", IsOptional = false, HasTag = false, HasDefaultValue = false)]
            public ControllingSequenceType Controlling
            {
                get
                {
                    return controlling_;
                }
                set
                {
                    selectControlling(value);
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


            public bool isNormalSelected()
            {
                return normal_selected;
            }


            public void selectNormal()
            {
                selectNormal(new NullObject());
            }


            public void selectNormal(NullObject val)
            {
                normal_ = val;
                normal_selected = true;


                controlling_selected = false;
            }


            public bool isControllingSelected()
            {
                return controlling_selected;
            }


            public void selectControlling(ControllingSequenceType val)
            {
                controlling_ = val;
                controlling_selected = true;


                normal_selected = false;
            }

            [ASN1PreparedElement]
            [ASN1Sequence(Name = "controlling", IsSet = false)]
            public class ControllingSequenceType : IASN1PreparedElement
            {
                private static IASN1PreparedElementData preparedData = CoderFactory.getInstance().newPreparedElementData(typeof(ControllingSequenceType));
                private ModeTypeChoiceType modeType_;


                [ASN1Element(Name = "modeType", IsOptional = false, HasTag = false, HasDefaultValue = false)]
                public ModeTypeChoiceType ModeType
                {
                    get
                    {
                        return modeType_;
                    }
                    set
                    {
                        modeType_ = value;
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

                [ASN1PreparedElement]
                [ASN1Choice(Name = "modeType")]
                public class ModeTypeChoiceType : IASN1PreparedElement
                {
                    private static IASN1PreparedElementData preparedData = CoderFactory.getInstance().newPreparedElementData(typeof(ModeTypeChoiceType));
                    private StartCount changeMode_;
                    private bool changeMode_selected;
                    private NullObject continueMode_;
                    private bool continueMode_selected;


                    [ASN1Null(Name = "continueMode")]
                    [ASN1Element(Name = "continueMode", IsOptional = false, HasTag = true, Tag = 0, HasDefaultValue = false)]
                    public NullObject ContinueMode
                    {
                        get
                        {
                            return continueMode_;
                        }
                        set
                        {
                            selectContinueMode(value);
                        }
                    }


                    [ASN1Element(Name = "changeMode", IsOptional = false, HasTag = true, Tag = 1, HasDefaultValue = false)]
                    public StartCount ChangeMode
                    {
                        get
                        {
                            return changeMode_;
                        }
                        set
                        {
                            selectChangeMode(value);
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


                    public bool isContinueModeSelected()
                    {
                        return continueMode_selected;
                    }


                    public void selectContinueMode()
                    {
                        selectContinueMode(new NullObject());
                    }


                    public void selectContinueMode(NullObject val)
                    {
                        continueMode_ = val;
                        continueMode_selected = true;


                        changeMode_selected = false;
                    }


                    public bool isChangeModeSelected()
                    {
                        return changeMode_selected;
                    }


                    public void selectChangeMode(StartCount val)
                    {
                        changeMode_ = val;
                        changeMode_selected = true;


                        continueMode_selected = false;
                    }
                }
            }
        }
    }
}