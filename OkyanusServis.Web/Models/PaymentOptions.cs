namespace OkyanusServis.Web.Models;

/// <summary>Ödeme formlarında kullanılan sabit seçenekler (yöntem, banka).</summary>
public static class PaymentOptions
{
    public static readonly string[] Methods =
    {
        "Nakit", "Elden", "Kredi Kartı", "Havale/EFT", "Taksit"
    };

    // Peşin / tek çekim yöntemleri (taksit yok — o ayrı alanda)
    public static readonly string[] DownMethods =
    {
        "Nakit", "Elden", "Kredi Kartı", "Havale/EFT"
    };

    public static readonly string[] Banks =
    {
        "Ziraat Bankası", "Garanti BBVA", "İş Bankası", "Akbank", "Yapı Kredi",
        "QNB", "Vakıfbank", "Halkbank", "Denizbank", "TEB", "ING", "Enpara",
        "Papara", "Diğer"
    };
}
