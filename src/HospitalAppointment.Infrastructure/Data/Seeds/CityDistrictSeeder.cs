using HospitalAppointment.Domain.Entities;
using HospitalAppointment.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HospitalAppointment.Infrastructure.Data.Seeds;

// Türkiye 81 il ve ilçe verisi
public static class CityDistrictSeeder
{
    public static async Task SeedAsync(AppDbContext context, ILogger logger)
    {
        if (await context.Cities.AnyAsync())
        {
            logger.LogInformation("İl/ilçe verisi zaten mevcut, atlanıyor.");
            return;
        }

        logger.LogInformation("İl ve ilçe verileri yükleniyor...");

        var data = GetCityData();
        foreach (var (plateCode, cityName, districts) in data)
        {
            var city = new City(plateCode, cityName);
            await context.Cities.AddAsync(city);
            await context.SaveChangesAsync();

            var districtEntities = districts
                .Select(d => new District(city.Id, d))
                .ToList();
            await context.Districts.AddRangeAsync(districtEntities);
        }

        await context.SaveChangesAsync();
        logger.LogInformation("{Count} il ve ilçeleri yüklendi.", data.Count);
    }

    private static List<(int Plate, string City, string[] Districts)> GetCityData() => new()
    {
        (1,  "Adana",          new[]{"Aladağ","Ceyhan","Çukurova","Feke","İmamoğlu","Karaisalı","Karataş","Kozan","Pozantı","Saimbeyli","Sarıçam","Seyhan","Tufanbeyli","Yumurtalık","Yüreğir"}),
        (2,  "Adıyaman",       new[]{"Adıyaman Merkez","Besni","Çelikhan","Gerger","Gölbaşı","Kahta","Samsat","Sincik","Tut"}),
        (3,  "Afyonkarahisar", new[]{"Afyonkarahisar Merkez","Başmakçı","Bayat","Bolvadin","Çay","Çobanlar","Dazkırı","Dinar","Emirdağ","Evciler","Hocalar","İhsaniye","İscehisar","Kızılören","Sandıklı","Sinanpaşa","Sultandağı","Şuhut"}),
        (4,  "Ağrı",           new[]{"Ağrı Merkez","Diyadin","Doğubayazıt","Eleşkirt","Hamur","Patnos","Taşlıçay","Tutak"}),
        (5,  "Amasya",         new[]{"Amasya Merkez","Göynücek","Gümüşhacıköy","Hamamözü","Merzifon","Suluova","Taşova"}),
        (6,  "Ankara",         new[]{"Akyurt","Altındağ","Ayaş","Bala","Beypazarı","Çamlıdere","Çankaya","Çubuk","Elmadağ","Etimesgut","Evren","Gölbaşı","Güdül","Haymana","Kalecik","Kahramankazan","Keçiören","Kızılcahamam","Mamak","Nallıhan","Polatlı","Pursaklar","Sincan","Şereflikoçhisar","Yenimahalle"}),
        (7,  "Antalya",        new[]{"Akseki","Aksu","Alanya","Demre","Döşemealtı","Elmalı","Finike","Gazipaşa","Gündoğmuş","İbradı","Kaş","Kemer","Kepez","Konyaaltı","Korkuteli","Kumluca","Manavgat","Muratpaşa","Serik"}),
        (8,  "Artvin",         new[]{"Ardanuç","Arhavi","Artvin Merkez","Borçka","Hopa","Kemalpaşa","Murgul","Şavşat","Yusufeli"}),
        (9,  "Aydın",          new[]{"Bozdoğan","Buharkent","Çine","Didim","Efeler","Germencik","İncirliova","Karacasu","Karpuzlu","Koçarlı","Köşk","Kuşadası","Kuyucak","Merkez","Nazilli","Söke","Sultanhisar","Yenipazar"}),
        (10, "Balıkesir",      new[]{"Altıeylül","Ayvalık","Balya","Bandırma","Bigadiç","Burhaniye","Dursunbey","Edremit","Erdek","Gömeç","Gönen","Havran","İvrindi","Karesi","Kepsut","Manyas","Marmara","Savaştepe","Sındırgı","Susurluk"}),
        (11, "Bilecik",        new[]{"Bilecik Merkez","Bozüyük","Gölpazarı","İnhisar","Osmaneli","Pazaryeri","Söğüt","Yenipazar"}),
        (12, "Bingöl",         new[]{"Adaklı","Bingöl Merkez","Genç","Karlıova","Kiğı","Solhan","Yayladere","Yedisu"}),
        (13, "Bitlis",         new[]{"Adilcevaz","Ahlat","Bitlis Merkez","Güroymak","Hizan","Mutki","Tatvan"}),
        (14, "Bolu",           new[]{"Bolu Merkez","Dörtdivan","Gerede","Göynük","Kıbrıscık","Mengen","Mudurnu","Seben","Yeniçağa"}),
        (15, "Burdur",         new[]{"Ağlasun","Altınyayla","Bucak","Burdur Merkez","Çavdır","Çeltikçi","Gölhisar","Kemer","Tefenni","Yeşilova"}),
        (16, "Bursa",          new[]{"Büyükorhan","Gemlik","Gürsu","Harmancık","İnegöl","İznik","Karacabey","Keles","Kestel","Mudanya","Mustafakemalpaşa","Nilüfer","Orhaneli","Orhangazi","Osmangazi","Yenişehir","Yıldırım"}),
        (17, "Çanakkale",      new[]{"Ayvacık","Bayramiç","Biga","Bozcaada","Çan","Çanakkale Merkez","Eceabat","Ezine","Gelibolu","Gökçeada","Lapseki","Yenice"}),
        (18, "Çankırı",        new[]{"Atkaracalar","Bayramören","Çerkeş","Eldivan","Ilgaz","Kızılırmak","Korgun","Kurşunlu","Orta","Şabanözü","Yapraklı","Çankırı Merkez"}),
        (19, "Çorum",          new[]{"Alaca","Bayat","Boğazkale","Dodurga","İskilip","Kargı","Laçin","Mecitözü","Oğuzlar","Ortaköy","Osmancık","Sungurlu","Uğurludağ","Çorum Merkez"}),
        (20, "Denizli",        new[]{"Acıpayam","Babadağ","Baklan","Bekilli","Beyağaç","Bozkurt","Buldan","Çal","Çameli","Çardak","Çivril","Güney","Honaz","Kale","Merkezefendi","Pamukkale","Sarayköy","Serinhisar","Tavas"}),
        (21, "Diyarbakır",     new[]{"Bağlar","Bismil","Çermik","Çınar","Çüngüş","Dicle","Eğil","Ergani","Hani","Hazro","Kayapınar","Kocaköy","Kulp","Lice","Silvan","Sur","Yenişehir"}),
        (22, "Edirne",         new[]{"Edirne Merkez","Enez","Havsa","İpsala","Keşan","Lalapaşa","Meriç","Süloğlu","Uzunköprü"}),
        (23, "Elazığ",         new[]{"Ağın","Alacakaya","Arıcak","Baskil","Elazığ Merkez","Karakoçan","Keban","Kovancılar","Maden","Palu","Sivrice"}),
        (24, "Erzincan",       new[]{"Çayırlı","Erzincan Merkez","İliç","Kemah","Kemaliye","Otlukbeli","Refahiye","Tercan","Üzümlü"}),
        (25, "Erzurum",        new[]{"Aşkale","Aziziye","Çat","Hınıs","Horasan","İspir","Karaçoban","Karayazı","Köprüköy","Narman","Oltu","Olur","Palandöken","Pasinler","Pazaryolu","Şenkaya","Tekman","Tortum","Uzundere","Yakutiye"}),
        (26, "Eskişehir",      new[]{"Alpu","Beylikova","Çifteler","Günyüzü","Han","İnönü","Mahmudiye","Mihalgazi","Mihalıççık","Odunpazarı","Sarıcakaya","Seyitgazi","Sivrihisar","Tepebaşı"}),
        (27, "Gaziantep",      new[]{"Araban","İslahiye","Karkamış","Nurdağı","Oğuzeli","Şahinbey","Şehitkamil","Nizip","Yavuzeli"}),
        (28, "Giresun",        new[]{"Alucra","Bulancak","Çamoluk","Çanakçı","Dereli","Doğankent","Espiye","Eynesil","Giresun Merkez","Görele","Güce","Keşap","Piraziz","Şebinkarahisar","Tirebolu","Yağlıdere"}),
        (29, "Gümüşhane",      new[]{"Gümüşhane Merkez","Kelkit","Köse","Kürtün","Şiran","Torul"}),
        (30, "Hakkari",        new[]{"Çukurca","Hakkari Merkez","Şemdinli","Yüksekova"}),
        (31, "Hatay",          new[]{"Altınözü","Antakya","Arsuz","Belen","Defne","Dörtyol","Erzin","Hassa","İskenderun","Kırıkhan","Kumlu","Payas","Reyhanlı","Samandağ","Yayladağı"}),
        (32, "Isparta",        new[]{"Aksu","Atabey","Eğirdir","Gelendost","Gönen","Keçiborlu","Isparta Merkez","Senirkent","Sütçüler","Şarkikaraağaç","Uluborlu","Yalvaç","Yenişarbademli"}),
        (33, "Mersin",         new[]{"Akdeniz","Anamur","Aydıncık","Bozyazı","Çamlıyayla","Erdemli","Gülnar","Mezitli","Mut","Silifke","Tarsus","Toroslar","Yenişehir"}),
        (34, "İstanbul",       new[]{"Adalar","Arnavutköy","Ataşehir","Avcılar","Bağcılar","Bahçelievler","Bakırköy","Başakşehir","Bayrampaşa","Beşiktaş","Beykoz","Beylikdüzü","Beyoğlu","Büyükçekmece","Çatalca","Çekmeköy","Esenler","Esenyurt","Eyüpsultan","Fatih","Gaziosmanpaşa","Güngören","Kadıköy","Kağıthane","Kartal","Küçükçekmece","Maltepe","Pendik","Sancaktepe","Sarıyer","Silivri","Sultanbeyli","Sultangazi","Şile","Şişli","Tuzla","Ümraniye","Üsküdar","Zeytinburnu"}),
        (35, "İzmir",          new[]{"Aliağa","Balçova","Bayındır","Bayraklı","Bergama","Beydağ","Bornova","Buca","Çeşme","Çiğli","Dikili","Foça","Gaziemir","Güzelbahçe","Karabağlar","Karaburun","Karşıyaka","Kemalpaşa","Kınık","Kiraz","Konak","Menderes","Menemen","Narlıdere","Ödemiş","Seferihisar","Selçuk","Tire","Torbalı","Urla"}),
        (36, "Kars",           new[]{"Akyaka","Arpaçay","Digor","Kağızman","Kars Merkez","Sarıkamış","Selim","Susuz"}),
        (37, "Kastamonu",      new[]{"Abana","Ağlı","Araç","Azdavay","Bozkurt","Cide","Çatalzeytin","Daday","Devrekani","Doğanyurt","Hanönü","İhsangazi","İnebolu","Kastamonu Merkez","Küre","Pınarbaşı","Seydiler","Şenpazar","Taşköprü","Tosya"}),
        (38, "Kayseri",        new[]{"Akkışla","Bünyan","Develi","Felahiye","Hacılar","İncesu","Kocasinan","Melikgazi","Özvatan","Pınarbaşı","Sarıoğlan","Sarız","Talas","Tomarza","Yahyalı","Yeşilhisar"}),
        (39, "Kırklareli",     new[]{"Babaeski","Demirköy","Kırklareli Merkez","Kofçaz","Lüleburgaz","Pehlivanköy","Pınarhisar","Vize"}),
        (40, "Kırşehir",       new[]{"Akçakent","Akpınar","Boztepe","Çiçekdağı","Kaman","Kırşehir Merkez","Mucur"}),
        (41, "Kocaeli",        new[]{"Başiskele","Çayırova","Darıca","Derince","Dilovası","Gebze","Gölcük","İzmit","Kandıra","Karamürsel","Kartepe","Körfez"}),
        (42, "Konya",          new[]{"Ahırlı","Akören","Akşehir","Altınekin","Beyşehir","Bozkır","Cihanbeyli","Çeltik","Çumra","Derbent","Derebucak","Doğanhisar","Emirgazi","Ereğli","Güneysınır","Hadim","Halkapınar","Hüyük","Ilgın","İvriz","Kadınhanı","Karapınar","Karatay","Kulu","Meram","Sarayönü","Selçuklu","Seydişehir","Taşkent","Tuzlukçu","Yalıhüyük","Yunak"}),
        (43, "Kütahya",        new[]{"Altıntaş","Aslanapa","Çavdarhisar","Domaniç","Dumlupınar","Emet","Gediz","Hisarcık","Kütahya Merkez","Pazarlar","Simav","Şaphane","Tavşanlı"}),
        (44, "Malatya",        new[]{"Akçadağ","Arapgir","Arguvan","Battalgazi","Darende","Doğanyol","Doğanşehir","Hekimhan","Kale","Kuluncak","Pütürge","Yazıhan","Yeşilyurt"}),
        (45, "Manisa",         new[]{"Ahmetli","Akhisar","Alaşehir","Demirci","Gölmarmara","Gördes","Kırkağaç","Köprübaşı","Kula","Salihli","Sarıgöl","Saruhanlı","Selendi","Soma","Şehzadeler","Turgutlu","Yunusemre"}),
        (46, "Kahramanmaraş",  new[]{"Afşin","Andırın","Çağlayancerit","Dulkadiroğlu","Ekinözü","Elbistan","Göksun","Nurhak","Onikişubat","Pazarcık","Türkoğlu"}),
        (47, "Mardin",         new[]{"Artuklu","Dargeçit","Derik","Kızıltepe","Mazıdağı","Midyat","Nusaybin","Ömerli","Savur","Yeşilli"}),
        (48, "Muğla",          new[]{"Bodrum","Dalaman","Datça","Fethiye","Kavaklıdere","Köyceğiz","Marmaris","Menteşe","Milas","Ortaca","Seydikemer","Ula","Yatağan"}),
        (49, "Muş",            new[]{"Bulanık","Hasköy","Korkut","Malazgirt","Muş Merkez","Varto"}),
        (50, "Nevşehir",       new[]{"Acıgöl","Avanos","Derinkuyu","Gülşehir","Hacıbektaş","Kozaklı","Nevşehir Merkez","Ürgüp"}),
        (51, "Niğde",          new[]{"Alt unkarahisar","Bor","Çamardı","Çiftlik","Niğde Merkez","Ulukışla"}),
        (52, "Ordu",           new[]{"Akkuş","Altınordu","Aybastı","Çamaş","Çatalpınar","Çaybaşı","Fatsa","Gölköy","Gülyalı","Gürgentepe","İkizce","Kabadüz","Kabataş","Korgan","Kumru","Mesudiye","Perşembe","Ulubey","Ünye"}),
        (53, "Rize",           new[]{"Ardeşen","Çamlıhemşin","Çayeli","Derepazarı","Fındıklı","Güneysu","Hemşin","İkizdere","İyidere","Kalkandere","Pazar","Rize Merkez"}),
        (54, "Sakarya",        new[]{"Adapazarı","Akyazı","Arifiye","Erenler","Ferizli","Geyve","Hendek","Karapürçek","Karasu","Kaynarca","Kocaali","Mithatpaşa","Pamukova","Sapanca","Serdivan","Söğütlü","Taraklı"}),
        (55, "Samsun",         new[]{"19 Mayıs","Alaçam","Asarcık","Atakum","Ayvacık","Bafra","Canik","Çarşamba","Havza","İlkadım","Kavak","Ladik","Ondokuzmayıs","Salıpazarı","Tekkeköy","Terme","Vezirköprü","Yakakent"}),
        (56, "Siirt",          new[]{"Baykan","Eruh","Kurtalan","Pervari","Siirt Merkez","Şirvan","Tillo"}),
        (57, "Sinop",          new[]{"Ayancık","Boyabat","Dikmen","Durağan","Erfelek","Gerze","Saraydüzü","Sinop Merkez","Türkeli"}),
        (58, "Sivas",          new[]{"Akıncılar","Altınyayla","Divriği","Doğanşar","Gemerek","Gölova","Hafik","İmranlı","Kangal","Koyulhisar","Şarkışla","Suşehri","Ulaş","Sivas Merkez","Yıldızeli","Zara"}),
        (59, "Tekirdağ",       new[]{"Çerkezköy","Çorlu","Ergene","Hayrabolu","Kapaklı","Malkara","Marmaraereğlisi","Muratlı","Saray","Süleymanpaşa","Şarköy"}),
        (60, "Tokat",          new[]{"Almus","Artova","Başçiftlik","Erbaa","Niksar","Pazar","Reşadiye","Tokat Merkez","Turhal","Yeşilyurt","Zile"}),
        (61, "Trabzon",        new[]{"Akçaabat","Araklı","Arsin","Beşikdüzü","Çaykara","Çarşıbaşı","Dernekpazarı","Düzköy","Hayrat","Köprübaşı","Maçka","Araklı","Of","Ortahisar","Sürmene","Şalpazarı","Tonya","Vakfıkebir","Yomra"}),
        (62, "Tunceli",        new[]{"Çemişgezek","Hozat","Mazgirt","Nazımiye","Ovacık","Pertek","Pülümür","Tunceli Merkez"}),
        (63, "Şanlıurfa",      new[]{"Akçakale","Birecik","Bozova","Ceylanpınar","Eyyübiye","Halfeti","Haliliye","Harran","Hilvan","Karaköprü","Siverek","Suruç","Viranşehir"}),
        (64, "Uşak",           new[]{"Banaz","Eşme","Karahallı","Sivaslı","Ulubey","Uşak Merkez"}),
        (65, "Van",            new[]{"Bahçesaray","Başkale","Çaldıran","Çatak","Edremit","Erciş","Gevaş","Gürpınar","İpekyolu","Muradiye","Özalp","Saray","Tuşba"}),
        (66, "Yozgat",         new[]{"Akdağmadeni","Aydıncık","Boğazlıyan","Çandır","Çayıralan","Çekerek","Kadışehri","Saraykent","Sarıkaya","Sorgun","Şefaatli","Yenifakılı","Yerköy","Yozgat Merkez"}),
        (67, "Zonguldak",      new[]{"Alaplı","Çaycuma","Devrek","Ereğli","Gökçebey","Kilimli","Kozlu","Zonguldak Merkez"}),
        (68, "Aksaray",        new[]{"Ağaçören","Eskil","Gülağaç","Güzelyurt","Ortaköy","Sarıyahşi","Sultanhanı","Aksaray Merkez"}),
        (69, "Bayburt",        new[]{"Aydıntepe","Demirözü","Bayburt Merkez"}),
        (70, "Karaman",        new[]{"Ayrancı","Başyayla","Ermenek","Kazımkarabekir","Sarıveliler","Karaman Merkez"}),
        (71, "Kırıkkale",      new[]{"Bahşili","Balışeyh","Çelebi","Delice","Karakeçili","Keskin","Sulakyurt","Yahşihan","Kırıkkale Merkez"}),
        (72, "Batman",         new[]{"Beşiri","Gercüş","Hasankeyf","Kozluk","Sason","Batman Merkez"}),
        (73, "Şırnak",         new[]{"Beytüşşebap","Cizre","Güçlükonak","İdil","Silopi","Uludere","Şırnak Merkez"}),
        (74, "Bartın",         new[]{"Amasra","Kurucaşile","Ulus","Bartın Merkez"}),
        (75, "Ardahan",        new[]{"Çıldır","Damal","Göle","Hanak","Posof","Ardahan Merkez"}),
        (76, "Iğdır",          new[]{"Aralık","Karakoyunlu","Tuzluca","Iğdır Merkez"}),
        (77, "Yalova",         new[]{"Altınova","Armutlu","Çınarcık","Çiftlikköy","Termal","Yalova Merkez"}),
        (78, "Karabük",        new[]{"Eflani","Eskipazar","Ovacık","Safranbolu","Yenice","Karabük Merkez"}),
        (79, "Kilis",          new[]{"Elbeyli","Musabeyli","Polateli","Kilis Merkez"}),
        (80, "Osmaniye",       new[]{"Bahçe","Düziçi","Hasanbeyli","Kadirli","Sumbas","Toprakkale","Osmaniye Merkez"}),
        (81, "Düzce",          new[]{"Akçakoca","Cumayeri","Çilimli","Gölyaka","Gümüşova","Kaynaşlı","Yığılca","Düzce Merkez"}),
    };
}
