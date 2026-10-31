using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

internal static class Language {
    static readonly string path=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"AirPodsBattery","language.txt");
    public static bool Russian=Load();
    static readonly Dictionary<string,string> translations=new Dictionary<string,string> {
        {"Left earbud","Левый наушник"},
        {"Right earbud","Правый наушник"},
        {" battery"," — заряд"},
        {"Waiting for data","Ожидание данных"},
        {"Approximate battery levels","Примерный уровень заряда"},
        {"Rescan","Поиск"},
        {"Hide device settings","Скрыть настройки устройств"},
        {"Device settings","Настройки устройств"},
        {"Show all AirPods models","Показать все модели AirPods"},
        {"Windows Bluetooth device","Устройство Bluetooth в Windows"},
        {"Windows Bluetooth device to monitor","Устройство Bluetooth для проверки подключения"},
        {"Nearby battery broadcast","Сигналы AirPods поблизости"},
        {"Select your AirPods","Выберите свои AirPods"},
        {"Choose your AirPods","Выберите свои AirPods"},
        {"Inactive entries expire after 2 minutes","Неактивные записи исчезнут через 2 мин"},
        {"AirPods: waiting for battery data","AirPods: ожидание данных о заряде"},
        {"Show battery","Показать заряд"},
        {"Restart scan","Повторить поиск"},
        {"Exit","Выход"},
        {"Check for updates","Проверить обновления"},
        {"GitHub releases","Версии на GitHub"},
        {"Bluetooth scan stopped: ","Поиск Bluetooth остановлен: "},
        {"No paired Bluetooth device found. Pair your AirPods in Windows.","Нет сопряжённых устройств. Подключите AirPods в Windows."},
        {"Unable to check Bluetooth connection. Try Rescan.","Не удалось проверить подключение. Повторите поиск."},
        {"You have the latest downloadable version (","У вас последняя доступная версия ("},
        {" is available. Open the release page?"," доступна. Открыть страницу версии?"},
        {"Update available","Доступно обновление"},
        {"AirPods Battery update available","Доступно обновление AirPods Battery"},
        {"Version ","Версия "},
        {" is available. Click to open the download page."," доступна. Нажмите, чтобы открыть страницу загрузки."},
        {"Could not check GitHub for updates. Check your internet connection and try again later.","Не удалось проверить обновления на GitHub. Проверьте подключение к интернету и повторите попытку позже."},
        {"Could not open your browser. Visit github.com/elnurm/airpods-battery-windows/releases.","Не удалось открыть браузер. Перейдите на github.com/elnurm/airpods-battery-windows/releases."},
        {"Cannot read this device's connection. Try Rescan.","Не удалось определить подключение устройства. Повторите поиск."},
        {"Connection check unavailable. Try Rescan.","Проверка подключения недоступна. Повторите поиск."},
        {"AirPods disconnected","AirPods отключены"},
        {"Your AirPods disconnected from this PC. Battery readings have been cleared.","AirPods отключены от компьютера. Данные о заряде очищены."},
        {"Unable to scan: ","Не удалось начать поиск: "},
        {"L —","Л —"},
        {"R —","П —"},
        {"Disconnected","Отключено"},
        {"Checking connection","Проверка подключения"},
        {"Last known","Последние данные"},
        {"Charging","Заряжается"},
        {"Approximately ","Примерно "},
        {" · charging"," · заряжается"},
        {"\nUpdated ","\nОбновлено "},
        {" seconds"," сек."},
        {" minutes"," мин."},
        {" ago"," назад"},
        {" (last known)"," (последние данные)"},
        {" old"," устар."},
        {"AirPods: select a detected pair","AirPods: выберите найденную пару"},
        {"Searching for your AirPods…","Поиск ваших AirPods…"},
        {"Select your nearby pair above","Выберите свои AirPods в настройках"},
        {"●  Connected to this PC","●  Подключены к компьютеру"},
        {"Waiting for updates · rescan or select your pair again","Нет новых данных · повторите поиск или выберите AirPods"},
        {"Searching for a unique matching broadcast… Multiple matches? Choose in Device settings.","Поиск подходящего сигнала… Если их несколько, выберите в настройках."},
        {"AirPods disconnected · reconnect in Windows","AirPods отключены · подключите их в Windows"},
        {"Select your Windows AirPods device in the top list","Выберите устройство AirPods в верхнем списке"},
        {"Checking Bluetooth connection…","Проверка подключения Bluetooth…"},
        {"AirPods: connection not verified","AirPods: подключение не подтверждено"},
        {"Seen just now","Сигнал только что"},
        {"Last seen ","Последний сигнал "},
        {"s ago"," сек. назад"},
        {"m ago"," мин. назад"},
        {"signal unavailable","сигнал недоступен"},
        {" (experimental)"," (экспериментально)"}
    };
    static bool Load() {
        try {if(File.Exists(path))return File.ReadAllText(path).Trim()=="ru";}
        catch(IOException) {} catch(UnauthorizedAccessException) {}
        return CultureInfo.CurrentUICulture.TwoLetterISOLanguageName=="ru";
    }
    public static void Set(bool russian,bool save) {
        Russian=russian;
        if(save) {
            try {Directory.CreateDirectory(Path.GetDirectoryName(path));File.WriteAllText(path,russian?"ru":"en");}
            catch(IOException) {} catch(UnauthorizedAccessException) {}
        }
    }
    public static string T(string text) {
        string translated;
        return Russian && translations.TryGetValue(text,out translated)?translated:text;
    }
    static string Original(string text) {
        foreach(var pair in translations)if(pair.Value==text)return pair.Key;
        return text;
    }
    public static string StoredMessage(string text) {
        text=Original(text);
        foreach(string prefix in new [] {"Bluetooth scan stopped: ","Unable to scan: "}) {
            string russian=translations[prefix];
            if(text.StartsWith(russian,StringComparison.Ordinal))text=prefix+text.Substring(russian.Length);
            if(text.StartsWith(prefix,StringComparison.Ordinal))return T(prefix)+text.Substring(prefix.Length);
        }
        return T(text);
    }
    public static void Apply(Control control) {
        if(!(control is ComboBox))control.Text=T(Original(control.Text));
        if(control.AccessibleName!=null)control.AccessibleName=T(Original(control.AccessibleName));
        foreach(Control child in control.Controls)Apply(child);
    }
    public static void ApplyMenu(ToolStripItem item) {
        item.Text=T(Original(item.Text));
        var menu=item as ToolStripMenuItem;
        if(menu!=null)foreach(ToolStripItem child in menu.DropDownItems)ApplyMenu(child);
    }
}

