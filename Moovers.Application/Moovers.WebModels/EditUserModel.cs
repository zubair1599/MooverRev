namespace Moovers.WebModels
{
    public class EditUserModel
    {
        public Business.Models.aspnet_Users_Profile UserProfile { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string DisplayName
        {
            get
            {
                return this.UserProfile.DisplayName();
            }
        }
        public string DisplayNumber
        {
            get
            {
                return this.UserProfile.DisplayNumber();
            }
        }
        public string OldPassword { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public string PictureURL { get; set; }
        public string Gender { get; set; }
        public EditUserModel()
        {
            this.UserProfile = null;
        }
        public EditUserModel(Business.Models.aspnet_Users_Profile userProfile)
            :this()
        {
            this.UserProfile = userProfile;
            this.FirstName = userProfile.FirstName;
            this.LastName = userProfile.LastName;          
            this.Email = userProfile.aspnet_Users.aspnet_Membership.Email;
            this.PictureURL = userProfile.PictureUrl;
           // this.Gender = userProfile.aspnet_Users

        }
    }
}
