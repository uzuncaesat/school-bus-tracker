using OkyanusServis.Api.Entities;

namespace OkyanusServis.Api.Data;

/// <summary>Veritabanına başlangıç verisi ekler (yoksa). Uygulama açılışında çağrılır.</summary>
public static class DbSeeder
{
    public static void Seed(AppDbContext db)
    {
        // Standart araçlar — plakası olmayanları ekle (var olanı çoğaltmaz)
        var wanted = new[]
        {
            new Vehicle { Plate = "34 SVR 341", Driver = "Hasan Yıldız", Phone = "0532 111 22 33", Capacity = 12, Color = "#0e7c7b" },
            new Vehicle { Plate = "34 OKY 208", Driver = "Kemal Aksoy",  Phone = "0533 444 55 66", Capacity = 12, Color = "#7a6cf0" },
            new Vehicle { Plate = "34 BUS 517", Driver = "Ramazan Er",   Phone = "0505 777 88 99", Capacity = 10, Color = "#f0a028" },
        };

        var mevcutPlakalar = db.Vehicles.Select(v => v.Plate).ToHashSet();
        foreach (var v in wanted)
            if (!mevcutPlakalar.Contains(v.Plate))
                db.Vehicles.Add(v);

        // Yaklaşım/takip ayarları: tek satır (Id=1) yoksa varsayılanlarla oluştur.
        if (!db.ServiceConfigs.Any())
            db.ServiceConfigs.Add(new ServiceConfig());

        db.SaveChanges();
    }
}
