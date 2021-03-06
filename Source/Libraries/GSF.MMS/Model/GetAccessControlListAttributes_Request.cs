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
    [ASN1Choice(Name = "GetAccessControlListAttributes_Request")]
    public class GetAccessControlListAttributes_Request : IASN1PreparedElement
    {
        private static readonly IASN1PreparedElementData preparedData = CoderFactory.getInstance().newPreparedElementData(typeof(GetAccessControlListAttributes_Request));
        private Identifier accessControlListName_;
        private bool accessControlListName_selected;
        private NamedObjectSequenceType namedObject_;
        private bool namedObject_selected;


        private NullObject vMD_;
        private bool vMD_selected;

        [ASN1Element(Name = "accessControlListName", IsOptional = false, HasTag = true, Tag = 0, HasDefaultValue = false)]
        public Identifier AccessControlListName
        {
            get
            {
                return accessControlListName_;
            }
            set
            {
                selectAccessControlListName(value);
            }
        }


        [ASN1Null(Name = "vMD")]
        [ASN1Element(Name = "vMD", IsOptional = false, HasTag = true, Tag = 1, HasDefaultValue = false)]
        public NullObject VMD
        {
            get
            {
                return vMD_;
            }
            set
            {
                selectVMD(value);
            }
        }


        [ASN1Element(Name = "namedObject", IsOptional = false, HasTag = true, Tag = 2, HasDefaultValue = false)]
        public NamedObjectSequenceType NamedObject
        {
            get
            {
                return namedObject_;
            }
            set
            {
                selectNamedObject(value);
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


        public bool isAccessControlListNameSelected()
        {
            return accessControlListName_selected;
        }


        public void selectAccessControlListName(Identifier val)
        {
            accessControlListName_ = val;
            accessControlListName_selected = true;


            vMD_selected = false;

            namedObject_selected = false;
        }


        public bool isVMDSelected()
        {
            return vMD_selected;
        }


        public void selectVMD()
        {
            selectVMD(new NullObject());
        }


        public void selectVMD(NullObject val)
        {
            vMD_ = val;
            vMD_selected = true;


            accessControlListName_selected = false;

            namedObject_selected = false;
        }


        public bool isNamedObjectSelected()
        {
            return namedObject_selected;
        }


        public void selectNamedObject(NamedObjectSequenceType val)
        {
            namedObject_ = val;
            namedObject_selected = true;


            accessControlListName_selected = false;

            vMD_selected = false;
        }

        [ASN1PreparedElement]
        [ASN1Sequence(Name = "namedObject", IsSet = false)]
        public class NamedObjectSequenceType : IASN1PreparedElement
        {
            private static IASN1PreparedElementData preparedData = CoderFactory.getInstance().newPreparedElementData(typeof(NamedObjectSequenceType));
            private ObjectClass objectClass_;


            private ObjectName objectName_;

            [ASN1Element(Name = "objectClass", IsOptional = false, HasTag = true, Tag = 0, HasDefaultValue = false)]
            public ObjectClass ObjectClass
            {
                get
                {
                    return objectClass_;
                }
                set
                {
                    objectClass_ = value;
                }
            }

            [ASN1Element(Name = "objectName", IsOptional = false, HasTag = true, Tag = 1, HasDefaultValue = false)]
            public ObjectName ObjectName
            {
                get
                {
                    return objectName_;
                }
                set
                {
                    objectName_ = value;
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