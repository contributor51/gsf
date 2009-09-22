//*******************************************************************************************************
//  MetadataRecordSummary.cs - Gbtc
//
//  Tennessee Valley Authority, 2009
//  No copyright is claimed pursuant to 17 USC � 105.  All Other Rights Reserved.
//
//  This software is made freely available under the TVA Open Source Agreement (see below).
//
//  Code Modification History:
//  -----------------------------------------------------------------------------------------------------
//  06/14/2007 - Pinal C. Patel
//       Generated original version of code based on DatAWare system specifications by Brian B. Fox, TVA.
//  04/20/2009 - Pinal C. Patel
//       Converted to C#.
//  09/15/2009 - Stephen C. Wills
//       Added new header and license agreement.
//
//*******************************************************************************************************

#region [ TVA Open Source Agreement ]
/*

 THIS OPEN SOURCE AGREEMENT ("AGREEMENT") DEFINES THE RIGHTS OF USE,REPRODUCTION, DISTRIBUTION,
 MODIFICATION AND REDISTRIBUTION OF CERTAIN COMPUTER SOFTWARE ORIGINALLY RELEASED BY THE
 TENNESSEE VALLEY AUTHORITY, A CORPORATE AGENCY AND INSTRUMENTALITY OF THE UNITED STATES GOVERNMENT
 ("GOVERNMENT AGENCY"). GOVERNMENT AGENCY IS AN INTENDED THIRD-PARTY BENEFICIARY OF ALL SUBSEQUENT
 DISTRIBUTIONS OR REDISTRIBUTIONS OF THE SUBJECT SOFTWARE. ANYONE WHO USES, REPRODUCES, DISTRIBUTES,
 MODIFIES OR REDISTRIBUTES THE SUBJECT SOFTWARE, AS DEFINED HEREIN, OR ANY PART THEREOF, IS, BY THAT
 ACTION, ACCEPTING IN FULL THE RESPONSIBILITIES AND OBLIGATIONS CONTAINED IN THIS AGREEMENT.

 Original Software Designation: openPDC
 Original Software Title: The TVA Open Source Phasor Data Concentrator
 User Registration Requested. Please Visit https://naspi.tva.com/Registration/
 Point of Contact for Original Software: J. Ritchie Carroll <mailto:jrcarrol@tva.gov>

 1. DEFINITIONS

 A. "Contributor" means Government Agency, as the developer of the Original Software, and any entity
 that makes a Modification.

 B. "Covered Patents" mean patent claims licensable by a Contributor that are necessarily infringed by
 the use or sale of its Modification alone or when combined with the Subject Software.

 C. "Display" means the showing of a copy of the Subject Software, either directly or by means of an
 image, or any other device.

 D. "Distribution" means conveyance or transfer of the Subject Software, regardless of means, to
 another.

 E. "Larger Work" means computer software that combines Subject Software, or portions thereof, with
 software separate from the Subject Software that is not governed by the terms of this Agreement.

 F. "Modification" means any alteration of, including addition to or deletion from, the substance or
 structure of either the Original Software or Subject Software, and includes derivative works, as that
 term is defined in the Copyright Statute, 17 USC � 101. However, the act of including Subject Software
 as part of a Larger Work does not in and of itself constitute a Modification.

 G. "Original Software" means the computer software first released under this Agreement by Government
 Agency entitled openPDC, including source code, object code and accompanying documentation, if any.

 H. "Recipient" means anyone who acquires the Subject Software under this Agreement, including all
 Contributors.

 I. "Redistribution" means Distribution of the Subject Software after a Modification has been made.

 J. "Reproduction" means the making of a counterpart, image or copy of the Subject Software.

 K. "Sale" means the exchange of the Subject Software for money or equivalent value.

 L. "Subject Software" means the Original Software, Modifications, or any respective parts thereof.

 M. "Use" means the application or employment of the Subject Software for any purpose.

 2. GRANT OF RIGHTS

 A. Under Non-Patent Rights: Subject to the terms and conditions of this Agreement, each Contributor,
 with respect to its own contribution to the Subject Software, hereby grants to each Recipient a
 non-exclusive, world-wide, royalty-free license to engage in the following activities pertaining to
 the Subject Software:

 1. Use

 2. Distribution

 3. Reproduction

 4. Modification

 5. Redistribution

 6. Display

 B. Under Patent Rights: Subject to the terms and conditions of this Agreement, each Contributor, with
 respect to its own contribution to the Subject Software, hereby grants to each Recipient under Covered
 Patents a non-exclusive, world-wide, royalty-free license to engage in the following activities
 pertaining to the Subject Software:

 1. Use

 2. Distribution

 3. Reproduction

 4. Sale

 5. Offer for Sale

 C. The rights granted under Paragraph B. also apply to the combination of a Contributor's Modification
 and the Subject Software if, at the time the Modification is added by the Contributor, the addition of
 such Modification causes the combination to be covered by the Covered Patents. It does not apply to
 any other combinations that include a Modification. 

 D. The rights granted in Paragraphs A. and B. allow the Recipient to sublicense those same rights.
 Such sublicense must be under the same terms and conditions of this Agreement.

 3. OBLIGATIONS OF RECIPIENT

 A. Distribution or Redistribution of the Subject Software must be made under this Agreement except for
 additions covered under paragraph 3H. 

 1. Whenever a Recipient distributes or redistributes the Subject Software, a copy of this Agreement
 must be included with each copy of the Subject Software; and

 2. If Recipient distributes or redistributes the Subject Software in any form other than source code,
 Recipient must also make the source code freely available, and must provide with each copy of the
 Subject Software information on how to obtain the source code in a reasonable manner on or through a
 medium customarily used for software exchange.

 B. Each Recipient must ensure that the following copyright notice appears prominently in the Subject
 Software:

          No copyright is claimed pursuant to 17 USC � 105.  All Other Rights Reserved.

 C. Each Contributor must characterize its alteration of the Subject Software as a Modification and
 must identify itself as the originator of its Modification in a manner that reasonably allows
 subsequent Recipients to identify the originator of the Modification. In fulfillment of these
 requirements, Contributor must include a file (e.g., a change log file) that describes the alterations
 made and the date of the alterations, identifies Contributor as originator of the alterations, and
 consents to characterization of the alterations as a Modification, for example, by including a
 statement that the Modification is derived, directly or indirectly, from Original Software provided by
 Government Agency. Once consent is granted, it may not thereafter be revoked.

 D. A Contributor may add its own copyright notice to the Subject Software. Once a copyright notice has
 been added to the Subject Software, a Recipient may not remove it without the express permission of
 the Contributor who added the notice.

 E. A Recipient may not make any representation in the Subject Software or in any promotional,
 advertising or other material that may be construed as an endorsement by Government Agency or by any
 prior Recipient of any product or service provided by Recipient, or that may seek to obtain commercial
 advantage by the fact of Government Agency's or a prior Recipient's participation in this Agreement.

 F. In an effort to track usage and maintain accurate records of the Subject Software, each Recipient,
 upon receipt of the Subject Software, is requested to register with Government Agency by visiting the
 following website: https://naspi.tva.com/Registration/. Recipient's name and personal information
 shall be used for statistical purposes only. Once a Recipient makes a Modification available, it is
 requested that the Recipient inform Government Agency at the web site provided above how to access the
 Modification.

 G. Each Contributor represents that that its Modification does not violate any existing agreements,
 regulations, statutes or rules, and further that Contributor has sufficient rights to grant the rights
 conveyed by this Agreement.

 H. A Recipient may choose to offer, and to charge a fee for, warranty, support, indemnity and/or
 liability obligations to one or more other Recipients of the Subject Software. A Recipient may do so,
 however, only on its own behalf and not on behalf of Government Agency or any other Recipient. Such a
 Recipient must make it absolutely clear that any such warranty, support, indemnity and/or liability
 obligation is offered by that Recipient alone. Further, such Recipient agrees to indemnify Government
 Agency and every other Recipient for any liability incurred by them as a result of warranty, support,
 indemnity and/or liability offered by such Recipient.

 I. A Recipient may create a Larger Work by combining Subject Software with separate software not
 governed by the terms of this agreement and distribute the Larger Work as a single product. In such
 case, the Recipient must make sure Subject Software, or portions thereof, included in the Larger Work
 is subject to this Agreement.

 J. Notwithstanding any provisions contained herein, Recipient is hereby put on notice that export of
 any goods or technical data from the United States may require some form of export license from the
 U.S. Government. Failure to obtain necessary export licenses may result in criminal liability under
 U.S. laws. Government Agency neither represents that a license shall not be required nor that, if
 required, it shall be issued. Nothing granted herein provides any such export license.

 4. DISCLAIMER OF WARRANTIES AND LIABILITIES; WAIVER AND INDEMNIFICATION

 A. No Warranty: THE SUBJECT SOFTWARE IS PROVIDED "AS IS" WITHOUT ANY WARRANTY OF ANY KIND, EITHER
 EXPRESSED, IMPLIED, OR STATUTORY, INCLUDING, BUT NOT LIMITED TO, ANY WARRANTY THAT THE SUBJECT
 SOFTWARE WILL CONFORM TO SPECIFICATIONS, ANY IMPLIED WARRANTIES OF MERCHANTABILITY, FITNESS FOR A
 PARTICULAR PURPOSE, OR FREEDOM FROM INFRINGEMENT, ANY WARRANTY THAT THE SUBJECT SOFTWARE WILL BE ERROR
 FREE, OR ANY WARRANTY THAT DOCUMENTATION, IF PROVIDED, WILL CONFORM TO THE SUBJECT SOFTWARE. THIS
 AGREEMENT DOES NOT, IN ANY MANNER, CONSTITUTE AN ENDORSEMENT BY GOVERNMENT AGENCY OR ANY PRIOR
 RECIPIENT OF ANY RESULTS, RESULTING DESIGNS, HARDWARE, SOFTWARE PRODUCTS OR ANY OTHER APPLICATIONS
 RESULTING FROM USE OF THE SUBJECT SOFTWARE. FURTHER, GOVERNMENT AGENCY DISCLAIMS ALL WARRANTIES AND
 LIABILITIES REGARDING THIRD-PARTY SOFTWARE, IF PRESENT IN THE ORIGINAL SOFTWARE, AND DISTRIBUTES IT
 "AS IS."

 B. Waiver and Indemnity: RECIPIENT AGREES TO WAIVE ANY AND ALL CLAIMS AGAINST GOVERNMENT AGENCY, ITS
 AGENTS, EMPLOYEES, CONTRACTORS AND SUBCONTRACTORS, AS WELL AS ANY PRIOR RECIPIENT. IF RECIPIENT'S USE
 OF THE SUBJECT SOFTWARE RESULTS IN ANY LIABILITIES, DEMANDS, DAMAGES, EXPENSES OR LOSSES ARISING FROM
 SUCH USE, INCLUDING ANY DAMAGES FROM PRODUCTS BASED ON, OR RESULTING FROM, RECIPIENT'S USE OF THE
 SUBJECT SOFTWARE, RECIPIENT SHALL INDEMNIFY AND HOLD HARMLESS  GOVERNMENT AGENCY, ITS AGENTS,
 EMPLOYEES, CONTRACTORS AND SUBCONTRACTORS, AS WELL AS ANY PRIOR RECIPIENT, TO THE EXTENT PERMITTED BY
 LAW.  THE FOREGOING RELEASE AND INDEMNIFICATION SHALL APPLY EVEN IF THE LIABILITIES, DEMANDS, DAMAGES,
 EXPENSES OR LOSSES ARE CAUSED, OCCASIONED, OR CONTRIBUTED TO BY THE NEGLIGENCE, SOLE OR CONCURRENT, OF
 GOVERNMENT AGENCY OR ANY PRIOR RECIPIENT.  RECIPIENT'S SOLE REMEDY FOR ANY SUCH MATTER SHALL BE THE
 IMMEDIATE, UNILATERAL TERMINATION OF THIS AGREEMENT.

 5. GENERAL TERMS

 A. Termination: This Agreement and the rights granted hereunder will terminate automatically if a
 Recipient fails to comply with these terms and conditions, and fails to cure such noncompliance within
 thirty (30) days of becoming aware of such noncompliance. Upon termination, a Recipient agrees to
 immediately cease use and distribution of the Subject Software. All sublicenses to the Subject
 Software properly granted by the breaching Recipient shall survive any such termination of this
 Agreement.

 B. Severability: If any provision of this Agreement is invalid or unenforceable under applicable law,
 it shall not affect the validity or enforceability of the remainder of the terms of this Agreement.

 C. Applicable Law: This Agreement shall be subject to United States federal law only for all purposes,
 including, but not limited to, determining the validity of this Agreement, the meaning of its
 provisions and the rights, obligations and remedies of the parties.

 D. Entire Understanding: This Agreement constitutes the entire understanding and agreement of the
 parties relating to release of the Subject Software and may not be superseded, modified or amended
 except by further written agreement duly executed by the parties.

 E. Binding Authority: By accepting and using the Subject Software under this Agreement, a Recipient
 affirms its authority to bind the Recipient to all terms and conditions of this Agreement and that
 Recipient hereby agrees to all terms and conditions herein.

 F. Point of Contact: Any Recipient contact with Government Agency is to be directed to the designated
 representative as follows: J. Ritchie Carroll <mailto:jrcarrol@tva.gov>.

*/
#endregion

using System;
using TVA.Parsing;

namespace TVA.Historian.Files
{
    /// <summary>
    /// A class with a subset of information defined in <see cref="MetadataRecord"/>. The <see cref="BinaryImage"/> of 
    /// <see cref="MetadataRecordSummary"/> is sent back as a reply to <see cref="Historian.Packets.PacketType3"/> and 
    /// <see cref="Historian.Packets.PacketType4"/>.
    /// </summary>
    /// <seealso cref="MetadataRecord"/>
    public class MetadataRecordSummary : ISupportBinaryImage
    {
        // **************************************************************************************************
        // *                                        Binary Structure                                        *
        // **************************************************************************************************
        // * # Of Bytes Byte Index Data Type  Property Name                                                 *
        // * ---------- ---------- ---------- --------------------------------------------------------------*
        // * 4          0-3        Int32      HistorianID                                                   *
        // * 4          4-7        Single     ExceptionLimit                                                *
        // * 4          8-11       Int32      Enabled                                                       *
        // * 4          12-15      Single     HighWarning                                                   *
        // * 4          16-19      Single     LowWarning                                                    *
        // * 4          20-23      Single     HighAlarm                                                     *
        // * 4          24-27      Single     LowAlarm                                                      *
        // * 4          28-31      Single     HighRange                                                     *
        // * 4          32-35      Single     LowRange                                                      *
        // **************************************************************************************************

        #region [ Members ]

        // Constants

        /// <summary>
        /// Specifies the number of bytes in the binary image of <see cref="MetadataRecordSummary"/>.
        /// </summary>
        public const int ByteCount = 36;

        // Fields
        private int m_historianID;
        private float m_exceptionLimit;
        private int m_enabled;
        private float m_highWarning;
        private float m_lowWarning;
        private float m_highAlarm;
        private float m_lowAlarm;
        private float m_highRange;
        private float m_lowRange;

        #endregion

        #region [ Constructors ]

        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataRecordSummary"/> class.
        /// </summary>
        /// <param name="record">A <see cref="MetadataRecord"/> object.</param>
        public MetadataRecordSummary(MetadataRecord record)
        {
            HistorianID = record.HistorianID;
            ExceptionLimit = record.AnalogFields.ExceptionLimit;
            Enabled = record.GeneralFlags.Enabled;
            HighWarning = record.AnalogFields.HighWarning;
            LowWarning = record.AnalogFields.LowWarning;
            HighAlarm = record.AnalogFields.HighAlarm;
            LowAlarm = record.AnalogFields.LowAlarm;
            HighRange = record.AnalogFields.HighRange;
            LowRange = record.AnalogFields.LowRange;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MetadataRecordSummary"/> class.
        /// </summary>
        /// <param name="binaryImage">Binary image to be used for initializing <see cref="MetadataRecordSummary"/>.</param>
        /// <param name="startIndex">0-based starting index of initialization data in the <paramref name="binaryImage"/>.</param>
        /// <param name="length">Valid number of bytes in <paramref name="binaryImage"/> from <paramref name="startIndex"/>.</param>
        public MetadataRecordSummary(byte[] binaryImage, int startIndex, int length)
        {
            Initialize(binaryImage, startIndex, length);
        }

        #endregion

        #region [ Properties ]

        /// <summary>
        /// Same as <see cref="MetadataRecord.HistorianID"/>.
        /// </summary>
        /// <exception cref="ArgumentException">The value being assigned is not positive or -1.</exception>
        public int HistorianID
        {
            get
            {
                return m_historianID;
            }
            private set
            {
                if (value < 1 && value != -1)
                    throw new ArgumentException("Value must be positive or -1.");

                m_historianID = value;
            }
        }

        /// <summary>
        /// Same as <see cref="MetadataRecordAnalogFields.ExceptionLimit"/>.
        /// </summary>
        public float ExceptionLimit
        {
            get
            {
                return m_exceptionLimit;
            }
            private set
            {
                m_exceptionLimit = value;
            }
        }

        /// <summary>
        /// Same as <see cref="MetadataRecordGeneralFlags.Enabled"/>.
        /// </summary>
        public bool Enabled
        {
            get
            {
                return Convert.ToBoolean(m_enabled);
            }
            private set
            {
                m_enabled = Convert.ToInt32(value);
            }
        }

        /// <summary>
        /// Same as <see cref="MetadataRecordAnalogFields.HighWarning"/>.
        /// </summary>
        public float HighWarning
        {
            get
            {
                return m_highWarning;
            }
            private set
            {
                m_highWarning = value;
            }
        }

        /// <summary>
        /// Same as <see cref="MetadataRecordAnalogFields.LowWarning"/>.
        /// </summary>
        public float LowWarning
        {
            get
            {
                return m_lowWarning;
            }
            private set
            {
                m_lowWarning = value;
            }
        }

        /// <summary>
        /// Same as <see cref="MetadataRecordAnalogFields.HighAlarm"/>.
        /// </summary>
        public float HighAlarm
        {
            get
            {
                return m_highAlarm;
            }
            private set
            {
                m_highAlarm = value;
            }
        }

        /// <summary>
        /// Same as <see cref="MetadataRecordAnalogFields.LowAlarm"/>.
        /// </summary>
        public float LowAlarm
        {
            get
            {
                return m_lowAlarm;
            }
            private set
            {
                m_lowAlarm = value;
            }
        }

        /// <summary>
        /// Same as <see cref="MetadataRecordAnalogFields.HighRange"/>.
        /// </summary>
        public float HighRange
        {
            get
            {
                return m_highRange;
            }
            private set
            {
                m_highRange = value;
            }
        }

        /// <summary>
        /// Same as <see cref="MetadataRecordAnalogFields.LowRange"/>.
        /// </summary>
        public float LowRange
        {
            get
            {
                return m_lowRange;
            }
            private set
            {
                m_lowRange = value;
            }
        }

        /// <summary>
        /// Gets the length of the <see cref="BinaryImage"/>.
        /// </summary>
        public int BinaryLength
        {
            get
            {
                return ByteCount;
            }
        }

        /// <summary>
        /// Gets the binary representation of <see cref="MetadataRecordSummary"/>.
        /// </summary>
        public byte[] BinaryImage
        {
            get
            {
                byte[] image = new byte[ByteCount];

                Array.Copy(EndianOrder.LittleEndian.GetBytes(m_historianID), 0, image, 0, 4);
                Array.Copy(EndianOrder.LittleEndian.GetBytes(m_exceptionLimit), 0, image, 4, 4);
                Array.Copy(EndianOrder.LittleEndian.GetBytes(Convert.ToInt32(m_enabled)), 0, image, 8, 4);
                Array.Copy(EndianOrder.LittleEndian.GetBytes(m_highWarning), 0, image, 12, 4);
                Array.Copy(EndianOrder.LittleEndian.GetBytes(m_lowWarning), 0, image, 16, 4);
                Array.Copy(EndianOrder.LittleEndian.GetBytes(m_highAlarm), 0, image, 20, 4);
                Array.Copy(EndianOrder.LittleEndian.GetBytes(m_lowAlarm), 0, image, 24, 4);
                Array.Copy(EndianOrder.LittleEndian.GetBytes(m_highRange), 0, image, 28, 4);
                Array.Copy(EndianOrder.LittleEndian.GetBytes(m_lowRange), 0, image, 32, 4);

                return image;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Initializes <see cref="MetadataRecordSummary"/> from the specified <paramref name="binaryImage"/>.
        /// </summary>
        /// <param name="binaryImage">Binary image to be used for initializing <see cref="MetadataRecordSummary"/>.</param>
        /// <param name="startIndex">0-based starting index of initialization data in the <paramref name="binaryImage"/>.</param>
        /// <param name="length">Valid number of bytes in <paramref name="binaryImage"/> from <paramref name="startIndex"/>.</param>
        /// <returns>Number of bytes used from the <paramref name="binaryImage"/> for initializing <see cref="MetadataRecordSummary"/>.</returns>
        public int Initialize(byte[] binaryImage, int startIndex, int length)
        {
            if (length >= ByteCount)
            {
                // Binary image has sufficient data.
                HistorianID = EndianOrder.LittleEndian.ToInt32(binaryImage, startIndex + 0);
                ExceptionLimit = EndianOrder.LittleEndian.ToSingle(binaryImage, startIndex + 4);
                Enabled = Convert.ToBoolean(EndianOrder.LittleEndian.ToInt32(binaryImage, startIndex + 8));
                HighWarning = EndianOrder.LittleEndian.ToSingle(binaryImage, startIndex + 12);
                LowWarning = EndianOrder.LittleEndian.ToSingle(binaryImage, startIndex + 16);
                HighAlarm = EndianOrder.LittleEndian.ToSingle(binaryImage, startIndex + 20);
                LowAlarm = EndianOrder.LittleEndian.ToSingle(binaryImage, startIndex + 24);
                HighRange = EndianOrder.LittleEndian.ToSingle(binaryImage, startIndex + 28);
                LowRange = EndianOrder.LittleEndian.ToSingle(binaryImage, startIndex + 32);

                return ByteCount;
            }
            else
            {
                // Binary image does not have sufficient data.
                return 0;
            }
        }

        #endregion
    }
}
