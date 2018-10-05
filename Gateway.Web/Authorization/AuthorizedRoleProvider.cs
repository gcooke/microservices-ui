﻿using System.IO;
using System.Linq;
using System.Web.Security;
using System.Xml.Serialization;
using Bagl.Cib.MIT.Logging;
using Bagl.Cib.MSF.ClientAPI.Gateway;
using Bagl.Cib.MSF.Contracts.Model;
using CommonServiceLocator;

namespace Gateway.Web.Authorization
{
    public class AuthorizedRoleProvider : RoleProvider
    {
        private readonly ILogger _logger;

        public AuthorizedRoleProvider()
        {
            var loggingService = ServiceLocator.Current.GetInstance<ILoggingService>();
            _logger = loggingService.GetLogger(this);
        }

        public override bool IsUserInRole(string username, string roleName)
        {
            var roleService = ServiceLocator.Current.GetInstance<IRoleService>();

            //TODO: Temporary fix - NOT SECURE
            //var userDetail = roleService.GetUserDetail(username);
            var userDetail = GetUserDetail();

            if (userDetail != null)
            {
                return userDetail.HasRole(roleName);
            }

            return false;
        }

        public override string[] GetRolesForUser(string username)
        {
            var roleService = ServiceLocator.Current.GetInstance<IRoleService>();

            //TODO: Temporary fix - NOT SECURE
            //var userDetail = roleService.GetUserDetail(username);
            var userDetail = GetUserDetail();
            if (userDetail != null)
            {
                return userDetail.Roles
                    .Where(r => r.SystemName == "Redstone Dashboard")
                    .Select(r => r.Name).ToArray();
            }
            else
            {
                _logger.ErrorFormat("User {0} does not have roles defined for Dashboard access.", username);
            }
            return new string[] { };
        }

        public override void CreateRole(string roleName)
        {
            throw new System.NotImplementedException();
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new System.NotImplementedException();
        }

        public override bool RoleExists(string roleName)
        {
            throw new System.NotImplementedException();
        }

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            throw new System.NotImplementedException();
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new System.NotImplementedException();
        }

        public override string[] GetUsersInRole(string roleName)
        {
            throw new System.NotImplementedException();
        }

        public override string[] GetAllRoles()
        {
            throw new System.NotImplementedException();
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            throw new System.NotImplementedException();
        }

        public override string ApplicationName { get; set; }

        private UserDetail GetUserDetail()
        {
            var payload = "<UserDetail><Roles><Role><Id>17</Id><Name>Access</Name><SystemNameId>3</SystemNameId><SystemName>Redstone Dashboard</SystemName></Role><Role><Id>18</Id><Name>Security.Delete</Name><SystemNameId>3</SystemNameId><SystemName>Redstone Dashboard</SystemName></Role><Role><Id>19</Id><Name>Security.Modify</Name><SystemNameId>3</SystemNameId><SystemName>Redstone Dashboard</SystemName></Role><Role><Id>20</Id><Name>Security.View</Name><SystemNameId>3</SystemNameId><SystemName>Redstone Dashboard</SystemName></Role><Role><Id>22</Id><Name>PFE Report</Name><SystemNameId>2</SystemNameId><SystemName>Web Reports</SystemName></Role><Role><Id>35</Id><Name>Recon Report</Name><SystemNameId>2</SystemNameId><SystemName>Web Reports</SystemName></Role><Role><Id>37</Id><Name>Counterparty: PFE Report</Name><SystemNameId>4</SystemNameId><SystemName>Sigma Viewer</SystemName></Role><Role><Id>38</Id><Name>Risk Batch Report</Name><SystemNameId>2</SystemNameId><SystemName>Web Reports</SystemName></Role><Role><Id>39</Id><Name>Counterparty: Counterparty Metrics</Name><SystemNameId>4</SystemNameId><SystemName>Sigma Viewer</SystemName></Role><Role><Id>40</Id><Name>Counterparty: XVA Credit Risk</Name><SystemNameId>4</SystemNameId><SystemName>Sigma Viewer</SystemName></Role><Role><Id>41</Id><Name>Counterparty: Exposure Profiles</Name><SystemNameId>4</SystemNameId><SystemName>Sigma Viewer</SystemName></Role><Role><Id>42</Id><Name>Counterparty: XVA FX Vega</Name><SystemNameId>4</SystemNameId><SystemName>Sigma Viewer</SystemName></Role><Role><Id>43</Id><Name>Marginal Trades Report</Name><SystemNameId>2</SystemNameId><SystemName>Web Reports</SystemName></Role><Role><Id>44</Id><Name>Book</Name><SystemNameId>5</SystemNameId><SystemName>Excel Add-Ins</SystemName></Role><Role><Id>45</Id><Name>Counterparty</Name><SystemNameId>5</SystemNameId><SystemName>Excel Add-Ins</SystemName></Role><Role><Id>46</Id><Name>Treasury LCY FTP Curves</Name><SystemNameId>2</SystemNameId><SystemName>Web Reports</SystemName></Role><Role><Id>47</Id><Name>Counterparty: Trade Report</Name><SystemNameId>4</SystemNameId><SystemName>Sigma Viewer</SystemName></Role><Role><Id>48</Id><Name>Portfolio: Trade Report</Name><SystemNameId>4</SystemNameId><SystemName>Sigma Viewer</SystemName></Role><Role><Id>49</Id><Name>Development</Name><SystemNameId>6</SystemNameId><SystemName>Documents</SystemName></Role><Role><Id>50</Id><Name>Support</Name><SystemNameId>6</SystemNameId><SystemName>Documents</SystemName></Role><Role><Id>51</Id><Name>Attribution: XVA Report</Name><SystemNameId>4</SystemNameId><SystemName>Sigma Viewer</SystemName></Role><Role><Id>52</Id><Name>FIOptions: Trade Report</Name><SystemNameId>4</SystemNameId><SystemName>Sigma Viewer</SystemName></Role><Role><Id>53</Id><Name>FTP_SA</Name><SystemNameId>7</SystemNameId><SystemName>Upload Items</SystemName></Role><Role><Id>54</Id><Name>Uploads</Name><SystemNameId>5</SystemNameId><SystemName>Excel Add-Ins</SystemName></Role><Role><Id>55</Id><Name>GHS.OS.Basis.Deliverable_ROA</Name><SystemNameId>7</SystemNameId><SystemName>Upload Items</SystemName></Role><Role><Id>56</Id><Name>Counterparty: Counterparty Profile</Name><SystemNameId>4</SystemNameId><SystemName>Sigma Viewer</SystemName></Role><Role><Id>57</Id><Name>FIOptions: Cashflow Report</Name><SystemNameId>4</SystemNameId><SystemName>Sigma Viewer</SystemName></Role><Role><Id>58</Id><Name>FXOptions: Cashflow Report</Name><SystemNameId>4</SystemNameId><SystemName>Sigma Viewer</SystemName></Role><Role><Id>73</Id><Name>FIOptions: BenchmarkRisk Report</Name><SystemNameId>4</SystemNameId><SystemName>Sigma Viewer</SystemName></Role><Role><Id>74</Id><Name>Counterparty: Marginal Trades Report</Name><SystemNameId>4</SystemNameId><SystemName>Sigma Viewer</SystemName></Role><Role><Id>75</Id><Name>USD.CREDIT.MUROnshore_ROA</Name><SystemNameId>7</SystemNameId><SystemName>Upload Items</SystemName></Role><Role><Id>76</Id><Name>USD.CREDIT.TZSOnshore_ROA</Name><SystemNameId>7</SystemNameId><SystemName>Upload Items</SystemName></Role><Role><Id>77</Id><Name>FTSE_Vol</Name><SystemNameId>7</SystemNameId><SystemName>Upload Items</SystemName></Role><Role><Id>78</Id><Name>ZMW.OS.Basis.Deliverable_ROA</Name><SystemNameId>7</SystemNameId><SystemName>Upload Items</SystemName></Role><Role><Id>79</Id><Name>TZS.NBC.Basis.Deliverable_ROA</Name><SystemNameId>7</SystemNameId><SystemName>Upload Items</SystemName></Role><Role><Id>80</Id><Name>KES.OS.Basis.Deliverable_ROA</Name><SystemNameId>7</SystemNameId><SystemName>Upload Items</SystemName></Role><Role><Id>81</Id><Name>ZAR.Bond.Repo.GC</Name><SystemNameId>7</SystemNameId><SystemName>Upload Items</SystemName></Role><Role><Id>82</Id><Name>FXOptions: Trade Report</Name><SystemNameId>4</SystemNameId><SystemName>Sigma Viewer</SystemName></Role><Role><Id>84</Id><Name>Filters: Risk Filter</Name><SystemNameId>4</SystemNameId><SystemName>Sigma Viewer</SystemName></Role><Role><Id>85</Id><Name>Counterparty: FX Wrong Way Risk</Name><SystemNameId>4</SystemNameId><SystemName>Sigma Viewer</SystemName></Role><Role><Id>86</Id><Name>Counterparty: IR Wrong Way Risk</Name><SystemNameId>4</SystemNameId><SystemName>Sigma Viewer</SystemName></Role><Role><Id>87</Id><Name>Counterparty: PV Stresses</Name><SystemNameId>4</SystemNameId><SystemName>Sigma Viewer</SystemName></Role></Roles><Sites><string>SOUTH_AFRICA</string><string>UGANDA</string><string>ZAMBIA</string><string>GHANA</string><string>TANZANIA_NBC</string><string>KENYA</string><string>BOTSWANA</string><string>MOZAMBIQUE</string><string>MAURITIUS_ONSHORE</string><string>TANZANIA</string><string>SEYCHELLES</string><string>MAURITIUS_IBD</string></Sites><Portfolios><string>FX IRS Hedges</string><string>Markets</string></Portfolios><PortfolioInternalIds><long>548</long><long>2093</long></PortfolioInternalIds><ReadTime>0001-01-01T00:00:00</ReadTime><Currency>USD</Currency></UserDetail>";
            XmlSerializer serializer = new XmlSerializer(typeof(UserDetail));
            using (TextReader reader = new StringReader(payload))
            {
                return (UserDetail)serializer.Deserialize(reader);
            }
        }
    }
};