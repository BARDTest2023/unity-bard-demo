// Assets/Scripts/BardSessionData.cs

using UnityEngine;
using System;
using System.Web;

public class BardSessionData
{
    public string playSessionUuid;
    public int variant;
    public string inputMode;
    public string lang;
    public string testName;
    public int testRank;
    public string device;

    public static BardSessionData FromUrl(string url)
    {
        // Ha az URL üres vagy nincs (Editor), adjunk default teszt­adatokat
        if (string.IsNullOrEmpty(url))
        {
            Debug.LogWarning("[BardSessionData] No URL detected, using default test parameters");
            return new BardSessionData {
                playSessionUuid = "test-session-uuid",
                variant         = 1,
                inputMode       = "standard",
                lang            = "hu",
                testName        = "clickscore",
                testRank        = 1,
                device          = "standard"
            };
        }

        // Egyébként parse-oljuk a query string-t
        var uri    = new Uri(url);
        var qs     = HttpUtility.ParseQueryString(uri.Query);
        return new BardSessionData
        {
            playSessionUuid = qs["play_session_uuid"],
            variant         = int.Parse(qs["variant"]   ?? "1"),
            inputMode       = qs["input"]                ?? "standard",
            lang            = qs["lang"]                 ?? "hu",
            testName        = qs["testName"]             ?? "clickscore",
            testRank        = int.Parse(qs["testRank"]   ?? "1"),
            device          = qs["device"]               ?? "standard"
        };
    }
}
