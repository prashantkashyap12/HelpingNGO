using System.ComponentModel.DataAnnotations;

namespace userPanelOMR.model
{
    public class SingUps
    {
        [Key]
        public int Id { get; set; } = 0;
        public string sName { get; set; } = "";
        public string sEmail { get; set; } = "";
        public string sPassword { get; set; } = "";
        public int sContact { get; set; } = 0;
        public string sOtp { get; set; } = "";
        public string role { get; set; } = "";
        public DateTime ExpiryDate { get; set; }
        public bool IsVerified { get; set; } 
        
    }
}
