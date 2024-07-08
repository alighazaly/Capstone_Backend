using System.ComponentModel.DataAnnotations;

namespace Capstone_Backend.Models
{
    public class AppartmentDTO
    {
        public String Title { get; set; }
        public String Description { get; set; }
        public int Price { get; set; }
        public DateTime UploadDate { get; set; }

        public int NumberOfBedrooms { get; set; }
        public int NumberOfBathrooms { get; set; }
        public int NumberOfBeds { get; set; }
        public String Elevator { get; set; }
        public String Generator { get; set; }
        public double Area { get; set; }
        public int MasterBedrooms { get; set; }
        public String Garden { get; set; }
        public int WaterContainers { get; set; }
        public String Pool { get; set; }
        public String Guard { get; set; }
        public String Kitchen { get; set; }
        public String BbqGrill { get; set; }
        public String HotTube { get; set; }
        public String Wifi { get; set; }
        public String WorkSpace { get; set; }
        public String IndoorFirePlace { get; set; }
        public String SmokingAllowed { get; set; }
        public String Gym { get; set; }
        public int Tvs { get; set; }
        public int Parking { get; set; }
        public string OwnerId { get; set; }
        public string CategoryName { get; set; }
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public List<IFormFile> images { get; set; }
        public String airConditionner { get; set; }
        public String TypeOfPlace { get; set; }


    }
}
