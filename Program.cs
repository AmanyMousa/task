using System;
using System.Collections.Generic;
using System.Linq;

namespace VehicleApp
{
    #region base class
    public abstract class Vehicle
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public int MaxSpeed { get; set; }

      


        public abstract decimal CalculatePrice(int days);


        public virtual string GetDetails()
        {
            return $"Id: {Id}\nType: {GetType().Name}\nBrand: {Brand}\nModel: {Model}\nYear: {Year}\nMaxSpeed: {MaxSpeed} km/h";
        }
    }
    #endregion

    #region Car
    public class Car : Vehicle
    {
        public int NumberOfDoors { get; set; } = 4;
        public bool AirConditioning { get; set; } = true;

        // base by  age + per-day factor + feature premium(Doors, AC)
        public override decimal CalculatePrice(int days)
        {
            if (days <= 0) return 0;
            int age = DateTime.Now.Year - Year;

            decimal baseDaily = 50m; 
            decimal ageDiscount =  (decimal)(age) * 0.5m;
            decimal acPremium = AirConditioning ? 5m : 0m;
            decimal doorPremium = NumberOfDoors > 2 ? 2m : 0m;
            decimal daily = baseDaily - ageDiscount + acPremium + doorPremium;
            return daily * days;
        }


        public override string GetDetails()
        {
            return base.GetDetails() + $"\nNumberOfDoors: {NumberOfDoors}\nHasAirConditioning: {AirConditioning}\nRentalPrice(1 day): {CalculatePrice(1):C}";
        }
    }
    #endregion

    #region Motorcycle
    public class Motorcycle : Vehicle
    {
        public int EngineCc { get; set; } = 150;
        public bool Sidecar { get; set; } = false;

        public override decimal CalculatePrice(int days)
        {
            if (days <= 0) return 0;
            decimal baseDaily = 25m;
            decimal ccFactor = (decimal)EngineCc / 100m;
            decimal sidecarPremium = Sidecar ? 15m : 0m;
            decimal daily = baseDaily + ccFactor * 5m + sidecarPremium;
            return daily * days;
        }

        public override string GetDetails()
        {
            return base.GetDetails() + $"\nEngineCc: {EngineCc}\nHasSidecar: {Sidecar}\nRentalPrice(1 day): {CalculatePrice(1):C}";
        }
    }
    #endregion

    #region Truck
    public class Truck : Vehicle
    {
        public decimal LoadCapacityTon { get; set; } = 1m;
        public bool RequiresSpecialLicense { get; set; } = false;


        public override decimal CalculatePrice(int days)
        {
            if (days <= 0) return 0;
            decimal baseDaily = 80m;
            decimal capacityFactor = LoadCapacityTon * 20m;
            decimal licenseSurcharge = RequiresSpecialLicense ? 30m : 0m;
            decimal daily = baseDaily + capacityFactor + licenseSurcharge;
            return daily * days;
        }

        public override string GetDetails()
        {
            return base.GetDetails() + $"\nLoadCapacityTon: {LoadCapacityTon}\nRequiresSpecialLicense: {RequiresSpecialLicense}\nRentalPrice(1 day): {CalculatePrice(1):C}";
        }
    }
    #endregion
    #region memory  repository
    public interface IVehicleRepository
    {
        void Add(Vehicle vehicle);
        IEnumerable<Vehicle> GetAll();
        IEnumerable<Vehicle> FindByBrand(string brand);
        IEnumerable<Vehicle> FindByType(string typeName);
        Vehicle? GetById(Guid id);
        bool Remove(Guid id);
    }

    public class InMemoryVehicleRepository : IVehicleRepository
    {
        private readonly List<Vehicle> _vehicles = new();

        public void Add(Vehicle vehicle)
        {
            if (vehicle == null) throw new ArgumentNullException(nameof(vehicle));
            _vehicles.Add(vehicle);
        }

        public IEnumerable<Vehicle> GetAll() => _vehicles.ToList();

        public IEnumerable<Vehicle> FindByBrand(string brand)
        {
            if (string.IsNullOrWhiteSpace(brand)) return Enumerable.Empty<Vehicle>();
            return _vehicles.Where(v => v.Brand.Equals(brand, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public IEnumerable<Vehicle> FindByType(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName)) return Enumerable.Empty<Vehicle>();
            return _vehicles.Where(v => v.GetType().Name.Equals(typeName, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public Vehicle? GetById(Guid id) => _vehicles.FirstOrDefault(v => v.Id == id);

        public bool Remove(Guid id)
        {
            var v = GetById(id);
            if (v == null) return false;
            return _vehicles.Remove(v);
        }
    }
    #endregion
    #region service layer
    public class VehicleService
    {
        private readonly IVehicleRepository _repo;

        public VehicleService(IVehicleRepository repo)
        {
            _repo = repo;
        }

        public void AddVehicle(Vehicle vehicle) => _repo.Add(vehicle);
        public IEnumerable<Vehicle> GetAll() => _repo.GetAll();
        public IEnumerable<Vehicle> SearchByBrand(string brand) => _repo.FindByBrand(brand);
        public IEnumerable<Vehicle> SearchByType(string typeName) => _repo.FindByType(typeName);
        public bool RemoveById(Guid id) => _repo.Remove(id);
        public Vehicle? GetById(Guid id) => _repo.GetById(id);
    }

    #endregion
    #region main 
    internal class Program
    {
        static void Main(string[] args)
        {
            var repo = new InMemoryVehicleRepository();
            var service = new VehicleService(repo);

            SeedSampleData(service);

            while (true)
            {
                Console.WriteLine("\n=== Vehicle Rental System ===");
                Console.WriteLine("1. Add new vehicle");
                Console.WriteLine("2. View all vehicles");
                Console.WriteLine("3. Search by brand");
                Console.WriteLine("4. Search by type");
                Console.WriteLine("5. Remove vehicle by Id");
                Console.WriteLine("6. Calculate rental price for vehicle");
                Console.WriteLine("0. Exit");
                Console.Write("Choose an option: ");
                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        AddVehicleFlow(service);
                        break;
                    case "2":
                        DisplayAll(service);
                        break;
                    case "3":
                        SearchByBrandFlow(service);
                        break;
                    case "4":
                        SearchByTypeFlow(service);
                        break;
                    case "5":
                        RemoveByIdFlow(service);
                        break;
                    case "6":
                        CalculatePriceFlow(service);
                        break;
                    case "0":
                        Console.WriteLine("Goodbye!");
                        return;
                    default:
                        Console.WriteLine("Invalid option.");
                        break;
                }
            }
        }

        static void SeedSampleData(VehicleService service)
        {
            service.AddVehicle(new Car { Brand = "Toyota", Model = "Corolla", Year = 2018, MaxSpeed = 180, NumberOfDoors = 4, AirConditioning = true });
            service.AddVehicle(new Motorcycle { Brand = "Yamaha", Model = "MT-15", Year = 2020, MaxSpeed = 140, EngineCc = 155, Sidecar = false });
            service.AddVehicle(new Truck { Brand = "Mercedes", Model = "Actros", Year = 2016, MaxSpeed = 120, LoadCapacityTon = 8.5m, RequiresSpecialLicense = true });
        }

        static void AddVehicleFlow(VehicleService service)
        {
            Console.WriteLine("\nChoose vehicle type to add: 1=Car, 2=Motorcycle, 3=Truck");
            var t = Console.ReadLine();
            Console.Write("Brand: "); var brand = Console.ReadLine() ?? "";
            Console.Write("Model: "); var model = Console.ReadLine() ?? "";
            Console.Write("Year: "); int.TryParse(Console.ReadLine(), out int year);
            Console.Write("MaxSpeed (km/h): "); int.TryParse(Console.ReadLine(), out int maxSpeed);

            switch (t)
            {
                case "1":
                    var car = new Car
                    {
                        Brand = brand,
                        Model = model,
                        Year = year,
                        MaxSpeed = maxSpeed
                    };
                    Console.Write("Number of doors: "); int.TryParse(Console.ReadLine(), out int doors); car.NumberOfDoors = doors == 0 ? 4 : doors;
                    Console.Write("Has AC? (y/n): "); var ac = Console.ReadLine(); car.AirConditioning = (ac?.ToLower() == "y");
                    service.AddVehicle(car);
                    Console.WriteLine("Car added.");
                    break;

                case "2":
                    var moto = new Motorcycle
                    {
                        Brand = brand,
                        Model = model,
                        Year = year,
                        MaxSpeed = maxSpeed
                    };
                    Console.Write("Engine CC: "); int.TryParse(Console.ReadLine(), out int cc); moto.EngineCc = cc == 0 ? 150 : cc;
                    Console.Write("Has sidecar? (y/n): "); var sc = Console.ReadLine(); moto.Sidecar = (sc?.ToLower() == "y");
                    service.AddVehicle(moto);
                    Console.WriteLine("Motorcycle added.");
                    break;

                case "3":
                    var truck = new Truck
                    {
                        Brand = brand,
                        Model = model,
                        Year = year,
                        MaxSpeed = maxSpeed
                    };
                    Console.Write("Load capacity in tons (e.g., 3.5): "); decimal.TryParse(Console.ReadLine(), out decimal cap); truck.LoadCapacityTon = cap == 0 ? 1.0m : cap;
                    Console.Write("Requires special license? (y/n): "); var rl = Console.ReadLine(); truck.RequiresSpecialLicense = (rl?.ToLower() == "y");
                    service.AddVehicle(truck);
                    Console.WriteLine("Truck added.");
                    break;

                default:
                    Console.WriteLine("Unknown vehicle type.");
                    break;
            }
        }

        static void DisplayAll(VehicleService service)
        {
            var all = service.GetAll().ToList();
            if (!all.Any())
            {
                Console.WriteLine("No vehicles available.");
                return;
            }

            Console.WriteLine($"\nAll Vehicles (count: {all.Count}):\n");
            foreach (var v in all)
            {
                Console.WriteLine(v.GetDetails());
                Console.WriteLine("---------------------------");
            }
        }

        static void SearchByBrandFlow(VehicleService service)
        {
            Console.Write("Enter brand to search: ");
            var brand = Console.ReadLine() ?? "";
            var results = service.SearchByBrand(brand).ToList();
            if (!results.Any()) { Console.WriteLine("No results."); return; }
            Console.WriteLine($"Found {results.Count} result(s):");
            foreach (var r in results) Console.WriteLine(r.GetDetails() + "\n---");
        }

        static void SearchByTypeFlow(VehicleService service)
        {
            Console.Write("Enter type name to search (Car, Motorcycle, Truck): ");
            var type = Console.ReadLine() ?? "";
            var results = service.SearchByType(type).ToList();
            if (!results.Any()) { Console.WriteLine("No results."); return; }
            Console.WriteLine($"Found {results.Count} result(s):");
            foreach (var r in results) Console.WriteLine(r.GetDetails() + "\n---");
        }

        static void RemoveByIdFlow(VehicleService service)
        {
            Console.Write("Enter Id to remove: ");
            var idStr = Console.ReadLine();
            if (!Guid.TryParse(idStr, out Guid id))
            {
                Console.WriteLine("Invalid Id format.");
                return;
            }
            var ok = service.RemoveById(id);
            Console.WriteLine(ok ? "Removed successfully." : "Vehicle not found.");
        }

        static void CalculatePriceFlow(VehicleService service)
        {
            Console.Write("Enter vehicle Id: ");
            var idStr = Console.ReadLine();
            if (!Guid.TryParse(idStr, out Guid id))
            {
                Console.WriteLine("Invalid Id.");
                return;
            }
            var v = service.GetById(id);
            if (v == null) { Console.WriteLine("Vehicle not found."); return; }
            Console.Write("Number of rental days: "); if (!int.TryParse(Console.ReadLine(), out int days) || days <= 0) { Console.WriteLine("Invalid days."); return; }
            var price = v.CalculatePrice(days);
            Console.WriteLine($"Type: {v.GetType().Name}, Brand: {v.Brand}, Model: {v.Model}");
            Console.WriteLine($"Total rental price for {days} day(s): {price:C}");
        }
    }
    #endregion

}
