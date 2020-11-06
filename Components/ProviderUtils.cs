﻿using DotNetNuke.Entities.Portals;
using NBrightCore.common;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Components;
using Square.Connect.Api;
using Square.Connect.Model;
using System;
using System.Linq;
using DotNetNuke.Services.Log.EventLog;
using DotNetNuke.Entities.Users;
using DotNetNuke.Abstractions.Portals;

namespace OS_Square
{
    public class ProviderUtils
    {
        readonly static Square.Connect.Client.Configuration _config;
        readonly static string _locationId;
        private readonly static Boolean _verboseLogging = false; // This should only be switched to true in test environments
        private static readonly EventLogController _objEventLog;

        static ProviderUtils() {
            var settings = ProviderUtils.GetProviderSettings();
            var accessToken = NBrightCore.common.Security.Decrypt(PortalController.Instance.GetCurrentSettings().GUID.ToString(), settings.GetXmlProperty("genxml/textbox/accesstoken"));
            var url = settings.GetXmlPropertyBool("genxml/checkbox/sandboxmode") == true ? "https://connect.squareupsandbox.com" : "https://connect.squareup.com";
            var debugmode = settings.GetXmlPropertyBool("genxml/checkbox/debugmode");

            _config = new Square.Connect.Client.Configuration(new Square.Connect.Client.ApiClient(url)){ AccessToken = accessToken };

            // Get the default location or an exact name match as specified in the plugin settings
            var squareLocationName = settings.GetXmlProperty("genxml/textbox/locationname");
            
            if (_verboseLogging) {
                _objEventLog = new EventLogController();
                _objEventLog.AddLog("OS_Square Message", "Location Name : " + squareLocationName, PortalController.Instance.GetCurrentSettings(), UserController.Instance.GetCurrentUserInfo().UserID, EventLogController.EventLogType.ADMIN_ALERT);
                _objEventLog.AddLog("OS_Square Message", "Url : " + url, PortalController.Instance.GetCurrentSettings(), UserController.Instance.GetCurrentUserInfo().UserID, EventLogController.EventLogType.ADMIN_ALERT);
                _objEventLog.AddLog("OS_Square accessToken", "accessToken : " + accessToken, PortalController.Instance.GetCurrentSettings(), UserController.Instance.GetCurrentUserInfo().UserID, EventLogController.EventLogType.ADMIN_ALERT);
            }

            _locationId = GetLocationId(squareLocationName);
        }
        public static NBrightInfo GetProviderSettings()
        {
            var objCtrl = new NBrightBuyController();
            var info = objCtrl.GetPluginSinglePageData("OS_Squarepayment", "OS_SquarePAYMENT", Utils.GetCurrentCulture());
            return info;
        }
        
        public static CreatePaymentResponse GetChargeResponse(OrderData orderData, string nonce)
        {
            var settings = ProviderUtils.GetProviderSettings();
            var portalSettings = PortalController.Instance.GetCurrentSettings();
            var userId = UserController.Instance.GetCurrentUserInfo().UserID;
            // Every payment you process must have a unique idempotency key.
            // If you're unsure whether a particular payment succeeded, you can reattempt
            // it with the same idempotency key without worrying about double charging
            var uuid = IdempotencyKey();

            // TODO: improve stability by saving idempotency for possible 
            //       resubmission in the event of a network failure

            var appliedtotal = orderData.PurchaseInfo.GetXmlPropertyDouble("genxml/appliedtotal");
            var alreadypaid = orderData.PurchaseInfo.GetXmlPropertyDouble("genxml/alreadypaid");

            // Square uses the smallest denomination of the Currency being used
            // ie. when we take 1.00 USD then we will need to convert to 100
            var currencyFactor = 100;
            var currencyCode = settings.GetXmlProperty("genxml/dropdownlist/currencycode");

            //TODO: verify only yen requires an adjustment to the currency factor
            if (currencyCode == "JPY")
            {
                currencyFactor = 1;
            }

            var orderTotal = (int)((appliedtotal - alreadypaid) * currencyFactor);
            var amount = new Money(orderTotal, currencyCode);

            var storename = StoreSettings.Current.SettingsInfo.GetXmlProperty("genxml/textbox/storename");
            var note = storename + " Order Number: " + orderData.OrderNumber;

            if (_verboseLogging) {
                _objEventLog.AddLog("OS_Square message", "ApiKey : " + _config.Password, (IPortalSettings)portalSettings, userId, EventLogController.EventLogType.ADMIN_ALERT);
                _objEventLog.AddLog("OS_Square message", "AccessToken : " + _config.AccessToken, (IPortalSettings)portalSettings, userId, EventLogController.EventLogType.ADMIN_ALERT);
                _objEventLog.AddLog("OS_Square message", "Header : " + Newtonsoft.Json.JsonConvert.SerializeObject(_config.ApiClient.Configuration.DefaultHeader), (IPortalSettings)portalSettings, userId, EventLogController.EventLogType.ADMIN_ALERT);
            }

            // Creating Payment Request
            // NOTE: OS Order Id is passed in both the reference_id & note field
            // because the reference_id is not visible in the ui but can be found via the 
            // transactions api.
            var _paymentsApi = new PaymentsApi(_config);
            var body = new CreatePaymentRequest(AmountMoney: amount, IdempotencyKey: uuid,  SourceId: nonce, LocationId: _locationId, ReferenceId: orderData.OrderNumber, Note: note );
            try
            {
                var response = _paymentsApi.CreatePayment(body);
                return response;
            }
            catch (Exception ex)
            {
                //TODO: detect network failures.  Wait. Resubmit.
                _objEventLog.AddLog("OS_Square err ", "Message : " + ex.Message, (IPortalSettings)portalSettings, userId, EventLogController.EventLogType.ADMIN_ALERT);
                orderData.AddAuditMessage(ex.Message, "notes", UserController.Instance.GetCurrentUserInfo().Username, "False");
                //throw;
            }

            return null;
        }

        private static string GetLocationId(string squareLocationName)
        {

            LocationsApi _locationApi = new LocationsApi(_config);
            
            var locationList = _locationApi.ListLocations();

            if (locationList == null || locationList.Locations.Count < 1)
            {
                throw new Exception("No Locations found. Charge aborted.");
            }

            // Set the default location to the 1st location
            var myLocation = locationList.Locations[0];

            // Check if the plugin settings have an location name matching one in Square's list
            if (!string.IsNullOrWhiteSpace(squareLocationName))
            {
                var l = locationList.Locations.Where(x => x.Name.Equals(squareLocationName, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
                if (l != null)
                {
                    myLocation = l;
                }
            }

            return myLocation.Id;
        }

        public static string IdempotencyKey()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
