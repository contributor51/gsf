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
    [ASN1Sequence(Name = "ReadJournal_Response", IsSet = false)]
    public class ReadJournal_Response : IASN1PreparedElement
    {
        private static readonly IASN1PreparedElementData preparedData = CoderFactory.getInstance().newPreparedElementData(typeof(ReadJournal_Response));
        private ICollection<JournalEntry> listOfJournalEntry_;


        private bool moreFollows_;

        [ASN1SequenceOf(Name = "listOfJournalEntry", IsSetOf = false)]
        [ASN1Element(Name = "listOfJournalEntry", IsOptional = false, HasTag = true, Tag = 0, HasDefaultValue = false)]
        public ICollection<JournalEntry> ListOfJournalEntry
        {
            get
            {
                return listOfJournalEntry_;
            }
            set
            {
                listOfJournalEntry_ = value;
            }
        }

        [ASN1Boolean(Name = "")]
        [ASN1Element(Name = "moreFollows", IsOptional = false, HasTag = true, Tag = 1, HasDefaultValue = true)]
        public bool MoreFollows
        {
            get
            {
                return moreFollows_;
            }
            set
            {
                moreFollows_ = value;
            }
        }


        public void initWithDefaults()
        {
            bool param_MoreFollows =
                false;
            MoreFollows = param_MoreFollows;
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