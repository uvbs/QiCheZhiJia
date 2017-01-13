﻿using CsharpHttpHelper;
using CsharpHttpHelper.Enum;
using HtmlAgilityPack;
using Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace Aide
{
    public class YiChe
    {
        /// <summary>
        /// 登录页面
        /// </summary>
        string loginurl = "http://dealer.easypass.cn/LoginPage/DefaultLogin.aspx";

        /// <summary>
        /// 登录提交
        /// </summary>
        string postlogin = "http://dealer.easypass.cn/Interface/CheckUserNameInterface.aspx?UserName={0}&CheckCode={1}&Pwd={2}&autoLogin=true&r=0.20315851840735166&appid=00000000-0000-0000-0000-000000000000&urls=";

        string validateCode = "http://dealer.easypass.cn/LoginCheckCode.ashx";
        string homeindex = "http://dealer.easypass.cn/HomeIndex.aspx";
        /// <summary>
        /// 用户列表
        /// </summary>
        string useradmin = "http://dealer.easypass.cn/UserManager/UserAdmin.aspx";

        string app_Shangji = "";
        string app_Shangji_Cookie = "";
        string app_CheYiTong = "";
        string app_CheYiTong_Cookie = "";

        /// <summary>
        /// 公共线索
        /// </summary>
        string commonorder = "http://app.easypass.cn/lmsnew/CommonOrder.aspx?customer=1";

        string cookie = "";        
        private static string StrJS = "";
        string company = "";
        string viewstate = "";
        string viewrator = "";

        public YiChe(string js)
        {
            StrJS = js;
        }

        #region Html
        private HtmlDocument GetHtml(string url)
        {
            var item = new HttpItem()
            {
                URL = url,
                Method = "get",
                ContentType = "text/html",
                Cookie = cookie
            };
            return GetHtml(item);
        }

        private HtmlDocument Post(string url, string postdata)
        {
            HttpItem item = new HttpItem
            {
                URL = url,
                Postdata = postdata,
                Cookie = cookie,
                ContentType = "application/x-www-form-urlencoded",
                Method = "POST",
                UserAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; WOW64; Trident/5.0)",
            };
            return GetHtml(item);
        }

        private HtmlDocument GetHtml(HttpItem item)
        {
            HttpHelper http = new HttpHelper();
            HttpResult htmlr = http.GetHtml(item);
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlr.Html);
            return htmlDoc;
        }
        #endregion

        #region 登录
        public Image LoadValidateCode()
        {
            HttpItem item = new HttpItem()
            {
                URL = validateCode,
                Method = "get",
                ResultType = ResultType.Byte,
                Cookie = cookie
            };

            HttpHelper http = new HttpHelper();
            var result = http.GetHtml(item);
            cookie += HttpHelper.GetSmallCookie(result.Cookie);

            item = new HttpItem()
            {
                URL = result.RedirectUrl,
                Method = "get",
                ResultType = ResultType.Byte,
                Cookie = cookie
            };
            result = http.GetHtml(item);
            
            MemoryStream ms = new MemoryStream(result.ResultByte);
            return Bitmap.FromStream(ms, true);
        }

        public void GotoLoginPage()
        {
            var item = new HttpItem()
            {
                URL = loginurl
            };

            HttpHelper http = new HttpHelper();
            HttpResult result = http.GetHtml(item);
            cookie += HttpHelper.GetSmallCookie(result.Cookie);
        }

        public ViewResult Login(string userName, string passWord, string code)
        {
            var result = UserLogin(userName, passWord, code);
            if (result.Result)
            {
                result = LoadPersonalInfo(userName, passWord);
            }

            return result;
        }

        private ViewResult UserLogin(string userName, string passWord, string code)
        {
            var username = HttpHelper.URLEncode(userName, Encoding.UTF8);
            var password = HttpHelper.URLEncode(passWord, Encoding.UTF8);
            var url = string.Format(postlogin, username, code, password);

            var item = new HttpItem()
            {
                URL = url,
                Cookie = cookie
            };

            HttpHelper http = new HttpHelper();
            HttpResult htmlr = http.GetHtml(item);
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlr.Html);
                        
            ViewResult result = new ViewResult();
            result.Result = false;
            result.Exit = false;
            var strhtml = htmlDoc.DocumentNode.OuterHtml;
            if (strhtml.IndexOf("验证码") != -1)
            {
                result.Message = "验证码输入有误";
            }
            else if (strhtml.IndexOf("账号或密码不正确") != -1)
            {
                result.Message = "账号或密码不正确，请重新登录！";
            }
            else
            {
                cookie += HttpHelper.GetSmallCookie(htmlr.Cookie);
                result.Result = true;
                result.Exit = true;
                item = new HttpItem()
                {
                    URL = homeindex,
                    Cookie = cookie
                };
                http = new HttpHelper();
                htmlr = http.GetHtml(item);
                htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(htmlr.Html);
                cookie += HttpHelper.GetSmallCookie(htmlr.Cookie);
                company = htmlDoc.DocumentNode.SelectSingleNode("//div[@class=\"user_rank\"]/div/h3").InnerText.Trim();
                app_Shangji = "http://dealer.easypass.cn/" + htmlDoc.DocumentNode.SelectSingleNode("//*[@id=\"appList\"]/ul[2]/li[1]/a").GetAttributeValue("href", "");
                app_CheYiTong = "http://dealer.easypass.cn/" + htmlDoc.DocumentNode.SelectSingleNode("//*[@id=\"appList\"]/ul[1]/li[1]/a").GetAttributeValue("href", "");                
            }

            return result;
        }

        private ViewResult LoadPersonalInfo(string userName, string passWord)
        {
            ViewResult vresult = new ViewResult();

            var htmlDoc = GetHtml(useradmin);

            var rowcount = Convert.ToInt32(htmlDoc.DocumentNode.SelectSingleNode("//*[@id=\"ContentPlaceHolder1_UpdatePanel1\"]/div/div/div[3]/ul/li[2]/strong").InnerText.Trim());

            var infoformat = "姓名:{0};职位:{1};手机:{2}" + Environment.NewLine;
            StringBuilder sb = new StringBuilder(rowcount * 25);            

            var trs = htmlDoc.DocumentNode.SelectNodes("//*[@id=\"ContentPlaceHolder1_dgvUserList\"]/tr");
            for (int i = 1; i < trs.Count; i++)
            {
                var tr = trs[i];
                var name = tr.SelectSingleNode("//*[@id=\"ContentPlaceHolder1_dgvUserList_UserManageHead_"+ (i-1).ToString() +"\"]").InnerText;
                var rolename = tr.ChildNodes[2].InnerText.Trim();
                var phone = tr.ChildNodes[4].InnerText.Trim().Split('\r')[0];
                sb.AppendFormat(infoformat, name, rolename, phone);
            }            

            string type = "ctl00%24ContentPlaceHolder1%24dgvUserList%24ctl{0}%24hideAccountType";
            string level = "ctl00%24ContentPlaceHolder1%24dgvUserList%24ctl{0}%24hideAccountLevel";
            string id = "ctl00%24ContentPlaceHolder1%24dgvUserList%24ctl{0}%24hideAccountId";
            string admin = "ctl00%24ContentPlaceHolder1%24dgvUserList%24ctl{0}%24hideAccountAdmin";
            string roleid = "ctl00%24ContentPlaceHolder1%24dgvUserList%24ctl{0}%24hideAccountRoleId";

            #region 分页
            while (rowcount > 10)
            {
                var postdata = HttpHelper.URLEncode("ctl00$ContentPlaceHolder1$ScriptManager1=ctl00$ContentPlaceHolder1$UpdatePanel1|ctl00$ContentPlaceHolder1$AspnetPager1$AspNetPager1");
                postdata += "&HADRDCCID=" + HttpHelper.URLEncode(htmlDoc.DocumentNode.SelectSingleNode("//*[@id=\"HADRDCCID\"]").GetAttributeValue("value", ""));
                postdata += "&__EVENTTARGET=ctl00%24ContentPlaceHolder1%24AspnetPager1%24AspNetPager1";
                postdata += "&__EVENTARGUMENT=2";
                postdata += "&__VIEWSTATE=" + HttpHelper.URLEncode(htmlDoc.DocumentNode.SelectSingleNode("//*[@id=\"__VIEWSTATE\"]").GetAttributeValue("value", ""));
                postdata += "&__VIEWSTATEGENERATOR=" + htmlDoc.DocumentNode.SelectSingleNode("//*[@id=\"__VIEWSTATEGENERATOR\"]").GetAttributeValue("value", "");
                postdata += "&__VIEWSTATEENCRYPTED=";
                postdata += "&__EVENTVALIDATION=" + HttpHelper.URLEncode(htmlDoc.DocumentNode.SelectSingleNode("//*[@id=\"__EVENTVALIDATION\"]").GetAttributeValue("value", ""));
                postdata += "&ctl00%24imgUploadChangehidethumburl=&ctl00%24imgUploadChangehideUrl=&ctl00%24ContentPlaceHolder1%24ddlRoleName=&ctl00%24ContentPlaceHolder1%24txtUserName=&ctl00%24ContentPlaceHolder1%24txtUserMobile=";

                for (int i = 1; i < trs.Count; i++)
                {
                    var tr = trs[i];
                    var type_t = string.Format(type, (i + 1).ToString().PadLeft(2, '0'));
                    var level_t = string.Format(level, (i + 1).ToString().PadLeft(2, '0'));
                    var id_t = string.Format(id, (i + 1).ToString().PadLeft(2, '0'));
                    var admin_t = string.Format(admin, (i + 1).ToString().PadLeft(2, '0'));
                    var roleid_t = string.Format(roleid, (i + 1).ToString().PadLeft(2, '0'));
                    postdata += string.Format("&{0}={1}", type_t, tr.SelectSingleNode("//*[@name=\"" + HttpHelper.URLDecode(type_t) + "\"]").GetAttributeValue("value", ""));
                    postdata += string.Format("&{0}={1}", level_t, tr.SelectSingleNode("//*[@name=\"" + HttpHelper.URLDecode(level_t) + "\"]").GetAttributeValue("value", ""));
                    postdata += string.Format("&{0}={1}", id_t, tr.SelectSingleNode("//*[@name=\"" + HttpHelper.URLDecode(id_t) + "\"]").GetAttributeValue("value", ""));
                    postdata += string.Format("&{0}={1}", admin_t, tr.SelectSingleNode("//*[@name=\"" + HttpHelper.URLDecode(admin_t) + "\"]").GetAttributeValue("value", ""));
                    postdata += string.Format("&{0}={1}", roleid_t, tr.SelectSingleNode("//*[@name=\"" + HttpHelper.URLDecode(roleid_t) + "\"]").GetAttributeValue("value", ""));
                }
                postdata += "&aspnet1CurrentPageIndex=1&__ASYNCPOST=true";

                htmlDoc = Post(useradmin, postdata);
                trs = htmlDoc.DocumentNode.SelectNodes("//*[@id=\"ContentPlaceHolder1_dgvUserList\"]/tr");
                for (int i = 1; i < trs.Count; i++)
                {
                    var tr = trs[i];
                    var name = tr.SelectSingleNode("//*[@id=\"ContentPlaceHolder1_dgvUserList_UserManageHead_" + (i - 1).ToString() + "\"]").InnerText;
                    var rolename = tr.ChildNodes[2].InnerText.Trim();
                    var phone = tr.ChildNodes[4].InnerText.Trim().Split('\r')[0];
                    sb.AppendFormat(infoformat, name, rolename, phone);
                }
                rowcount -= 10;
            }
            #endregion

            Service.User user = new Service.User
            {
                Company = company,
                CompanyID = "",
                SiteName = "易车网",
                PassWord = passWord,
                UserName = userName,
                Status = 1,
                LinkInfo = sb.ToString()
            };

            var loginResult = Tool.service.UserLogin(user);

            if (loginResult.Result)
            {
                Tool.userInfo_yc = loginResult.Data;
            }
            else
            {
                vresult.Message = loginResult.Message;
                vresult.Exit = true;
            }
            vresult.Result = loginResult.Result;

            return vresult;
        }

        /// <summary>
        /// 读取本地的账号和密码
        /// </summary>
        public string[] LoadPw()
        {
            string[] str = new string[2];
            if (ConfigurationManager.AppSettings["chkSavePass_YC"] == "True")
            {
                str[0] = ConfigurationManager.AppSettings["UserName_YC"];
                str[1] = ConfigurationManager.AppSettings["PassWord_YC"];
            }
            return str;
        }

        /// <summary>
        /// 本地保存登录的账号和密码
        /// </summary>
        public void SavePw()
        {
            Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            configuration.AppSettings.Settings["UserName_YC"].Value = Tool.userInfo_qc.UserName;
            configuration.AppSettings.Settings["PassWord_YC"].Value = Tool.userInfo_qc.PassWord;
            configuration.AppSettings.Settings["chkSavePass_YC"].Value = "True";
            configuration.Save();
        }
        #endregion

        /// <summary>
        /// 站点跳转
        /// </summary>
        /// <returns></returns>
        private string OsLogin(string url)
        {
            var item = new HttpItem()
            {
                URL = url,
                Cookie = cookie
            };

            HttpHelper http = new HttpHelper();
            HttpResult htmlr = http.GetHtml(item);
            cookie += HttpHelper.GetSmallCookie(htmlr.Cookie);
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlr.Html);

            var form1 = htmlDoc.DocumentNode.SelectSingleNode("//*[@id=\"form1\"]").GetAttributeValue("action", "");
            var AppKey = htmlDoc.DocumentNode.SelectSingleNode("//*[@id=\"AppKey\"]").GetAttributeValue("value", "");
            var AppValue = htmlDoc.DocumentNode.SelectSingleNode("//*[@id=\"AppValue\"]").GetAttributeValue("value", "");
            var OP_UserID = htmlDoc.DocumentNode.SelectSingleNode("//*[@id=\"OP_UserID\"]").GetAttributeValue("value", "");
            var Check_Code = htmlDoc.DocumentNode.SelectSingleNode("//*[@id=\"Check_Code\"]").GetAttributeValue("value", "");
            var radomCode = htmlDoc.DocumentNode.SelectSingleNode("//*[@id=\"radomCode\"]").GetAttributeValue("value", "");
            var ClientIP = htmlDoc.DocumentNode.SelectSingleNode("//*[@id=\"ClientIP\"]").GetAttributeValue("value", "");
            var SuperFlag = htmlDoc.DocumentNode.SelectSingleNode("//*[@id=\"SuperFlag\"]").GetAttributeValue("value", "");
            var WeakOPUserID = htmlDoc.DocumentNode.SelectSingleNode("//*[@id=\"WeakOPUserID\"]").GetAttributeValue("value", "");
            var ImitateUserID = htmlDoc.DocumentNode.SelectSingleNode("//*[@id=\"ImitateUserID\"]").GetAttributeValue("value", "");

            var postdata = string.Format("AppKey={0}&AppValue={1}&OP_UserID={2}&Check_Code={3}&radomCode={4}&ClientIP={5}&SuperFlag={6}&WeakOPUserID={7}&ImitateUserID={8}&ClientTime={9}", AppKey, AppValue, OP_UserID, Check_Code, radomCode, ClientIP, SuperFlag, WeakOPUserID, ImitateUserID, DateTime.Now.ToString());

            item = new HttpItem
            {
                URL = form1,
                Postdata = postdata,
                Cookie = cookie,
                ContentType = "application/x-www-form-urlencoded",
                Method = "POST",
                UserAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; WOW64; Trident/5.0)",
            };
            htmlr = http.GetHtml(item);
            return HttpHelper.GetSmallCookie(htmlr.Cookie);            
        }

        #region 抢单
        /// <summary>
        /// 加载公共订单页面
        /// </summary>
        /// <returns></returns>
        public HtmlDocument GoToOrder()
        {
            app_Shangji_Cookie =  OsLogin(app_Shangji);

            var item = new HttpItem()
            {
                URL = commonorder,
                Cookie = app_Shangji_Cookie,
                Referer = "http://dealer.easypass.cn/gotoapp.aspx?appid=4e236245-4f49-4965-8f86-a490f8bfb657&r=636198532169784090&urls=http%3a%2f%2fapp.easypass.cn%2flmsnew%2fCommonOrder.aspx%3fcustomer%3d1"
            };
            var htmlDoc = GetHtml(item);

            viewstate = htmlDoc.DocumentNode.SelectSingleNode("//*[@id=\"__VIEWSTATE\"]").GetAttributeValue("value", "");
            viewrator = htmlDoc.DocumentNode.SelectSingleNode("//*[@id=\"__VIEWSTATEGENERATOR\"]").GetAttributeValue("value", "");
            return htmlDoc;
        }

        public HtmlDocument LoadCityByPro(string dealerid, string provid)
        {
            var item = new HttpItem
            {
                URL = "http://app.easypass.cn/lmsnew/ajaxLoad/AjaxRequest.aspx?op=GetLocationByDealerIdAndProvinceId",
                Postdata = "DealerId:"+ dealerid +"&ProvId=" + provid,
                Cookie = appcookie,
                ContentType = "application/x-www-form-urlencoded",
                Method = "POST",
                UserAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; WOW64; Trident/5.0)",
            };
            return GetHtml(item);
            
        }

        public HtmlDocument LoadOrder(string type, string pro, string city)
        {
            var postdata = "ScriptManager1=UpdatePanel1|btnSearch&hf_OrderType={0}&hf_Province={1}&hf_Location={2}&__EVENTTARGET=&__EVENTARGUMENT=&__VIEWSTATE={3}__VIEWSTATEGENERATOR={4}__VIEWSTATEENCRYPTED=&__ASYNCPOST=true&btnSearch=查询";

            var item = new HttpItem
            {
                URL = commonorder,
                Postdata = string.Format(postdata, type, pro, city, viewstate, viewrator),
                Cookie = app_Shangji_Cookie,
                ContentType = "application/x-www-form-urlencoded",
                Method = "POST",
                UserAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; WOW64; Trident/5.0)",
            };
            return GetHtml(item);
        }
        #endregion

        #region 报价
        private HtmlDocument GoToPrice()
        {
            app_CheYiTong_Cookie = OsLogin(app_CheYiTong);

            var item = new HttpItem()
            {
                URL = "http://das.app.easypass.cn/Price/PriceManage.aspx",
                Cookie = app_CheYiTong_Cookie
            };
            var htmlDoc = GetHtml(item);

            return htmlDoc;
        }

        public void SavePrice()
        {
            var htmlDoc = GoToPrice();

            var total = Convert.ToInt32(htmlDoc.DocumentNode.SelectSingleNode("//*[@id=\"table_cx\"]/table/tbody/tr[1]/th[2]/span/label").InnerText.Trim());

            while (total > 0)
            {
                var postdata = "ScriptManager1=UpdatePanel2|lbtnBatchUpdate&__EVENTTARGET=lbtnBatchUpdate&__EVENTARGUMENT=&__LASTFOCUS=&__VIEWSTATE={0}__VIEWSTATEGENERATOR={1}&__EVENTVALIDATION=&ddlCarSerial=0&ddlSMSPriceStatus=0";

                var trs = htmlDoc.DocumentNode.SelectNodes("//tr[contains(@id,'tr-price-')]");

                total -= trs.Count;

                var strpid = "rpDealerPriceList$ctl00$rpDealerPriceCarList$ctl{0:D2}$hfDPId";
                var strcarid = "rpDealerPriceList$ctl00$rpDealerPriceCarList$ctl{0:D2}$hfCarId";
                var strcsid = "rpDealerPriceList$ctl00$rpDealerPriceCarList$ctl{0:D2}$hfCsId";
                var strprice = "rpDealerPriceList$ctl00$rpDealerPriceCarList$ctl{0:D2}$txtSalePrice";

                for (int i = 0; i < trs.Count; i++)
                {
                    var pid = string.Format(strpid, i);
                    var carid = string.Format(strcarid, i);
                    var csid = string.Format(strcsid, i);
                    var price = string.Format(strprice, i);

                    postdata += "&" + pid + "=" + htmlDoc.DocumentNode.SelectSingleNode("//*[@id=\"" + pid + "\"]").GetAttributeValue("value", "");
                    postdata += "&" + carid + "=" + htmlDoc.DocumentNode.SelectSingleNode("//*[@id=\"" + carid + "\"]").GetAttributeValue("value", "");
                    postdata += "&" + csid + "=" + htmlDoc.DocumentNode.SelectSingleNode("//*[@id=\"" + csid + "\"]").GetAttributeValue("value", "");
                    postdata += "&" + price + "=" + htmlDoc.DocumentNode.SelectSingleNode("//*[@id=\"" + price + "\"]").GetAttributeValue("value", "");
                }

                postdata += "&hfDealerPriceId=&hfEditPriceCarId=&hfEditSMSPriceCarId=&hfWeekSuperLogin=&txtSMSPrice=&txtBeginDate=&txtEndDate=&ddlSMSPromotionCategory=1&ddlSMSPromotionType=1&txtPromotion=&txtBatchBeginDate=&txtBatchEndDate=&__ASYNCPOST=true";

                var item = new HttpItem
                {
                    URL = "http://das.app.easypass.cn/Price/PriceManage.aspx",
                    Postdata = postdata,
                    Cookie = app_CheYiTong_Cookie,
                    ContentType = "application/x-www-form-urlencoded",
                    Method = "POST",
                    UserAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; WOW64; Trident/5.0)",
                };
            }
        }
        #endregion
    }
}
