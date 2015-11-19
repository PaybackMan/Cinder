using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cinder.Core.Domain.Person
{
    public enum PractitionerSpecialty
    {
        Cardiologist,
        Dentist,
        DietaryConsultant,
        Midwife,
        SystemsArchitect
    }

    public enum PractitionerRole
    {
        Doctor,
        Nurse,
        Pharmacist,
        Researcher,
        Teacher, 
        Ict
    }

    public enum NameUse
    {
        /// <summary> Known as/conventional/the one you normally use.</summary>
        Usual,
        /// <summary> The formal name as registered in an official (government) registry, but which name might not be commonly used. May be called "legal name".</summary>     
        Official,
        /// <summary> A temporary name. Name.period can provide more detailed information. This may also be used for temporary names assigned at birth or in emergency situations.</summary>
        Temp,
        /// <summary>A name that is used to address the person in an informal manner, but is not part of their formal or usual name.</summary>
        NickName,
        /// <summary> Anonymous assigned name, alias, or pseudonym (used to protect a person's identity for privacy reasons).</summary>
        Anonymous,
        /// <summary>this name is no longer in use (or was never correct, but retained for records).</summary>
        Old,
        /// <summary>A name used prior to marriage. Marriage naming customs vary greatly around the world. This name use is for use by applications that collect and store "maiden" names. Though the concept of maiden name is often gender specific, the use of this term is not gender specific. The use of this term does not imply any particular history for a person's name, nor should the maiden name be determined algorithmically.</summary>
        Maiden      
    }

    public enum Gender
    {
        /// <summary>Fat mammels that like to break shit.</summary>
        Male,
        /// <summary>The Ladies!!</summary>
        Female,
        /// <summary>The gender of a person could not be uniquely defined as male or female, such as hermaphrodite. Usually named Pat</summary>
        Undifferentiated  
    }

    public enum MartialStatus
    {
        /// <summary>Marriage contract has been declared null and to not have existed</summary>
        Annulled,
        /// <summary> Marriage contract has been declared dissolved and inactive</summary>
        Divorced,
        /// <summary>Subject to an Interlocutory Decree.</summary>
        Interlocutory,      
        /// <summary>A current marriage contract is not active</summary>
        LegallySeparated,
        /// <summary>A current marriage contract is active</summary>
        Married,
        /// <summary>More than 1 current spouse</summary>
        Polygamous,
        /// <summary>No marriage contract has ever been entered</summary>
        NeverMarried,
        /// <summary>Person declares that a domestic partner relationship exists.</summary>
        DomesticPartner,
        /// <summary>Currently not in a marriage contract.</summary>
        Unmarried,
        /// <summary>The spouse has died</summary>
        Widowed           
    }
}
