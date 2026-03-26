using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using wnc.Features.Home.ViewModels;
using wnc.Infrastructure.Security;
using wnc.Models;

namespace wnc.Controllers;

public class HomeController(
    PersistentUserStateCookieService persistentCookieService,
    PortalSessionService portalSessionService) : Controller
{
    public IActionResult Index()
    {
        var persistentState = persistentCookieService.ReadFromRequest(Request);
        var favoriteFeature = persistentState.FeatureHits
            .OrderByDescending(x => x.Value)
            .ThenBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
            .Select(x => x.Key)
            .FirstOrDefault() ?? "Chua co du lieu";

        var sessionState = portalSessionService.GetSnapshot(HttpContext);

        return View(new HomeIndexViewModel
        {
            PersistentState = new PersistentUserStateViewModel
            {
                TotalVisits = persistentState.TotalVisits,
                LastVisitedAtUtc = persistentState.LastVisitedAtUtc,
                FavoriteFeature = favoriteFeature
            },
            SessionState = new SessionUserStateViewModel
            {
                RequestCount = sessionState.RequestCount,
                Portal = sessionState.Portal,
                DisplayName = sessionState.DisplayName,
                Roles = sessionState.Roles,
                AuthenticatedAtUtc = sessionState.AuthenticatedAtUtc,
                LastPath = sessionState.LastPath
            }
        });
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
