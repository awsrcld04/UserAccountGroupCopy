// Copyright (C) 2025 Akil Woolfolk Sr. 
// All Rights Reserved
// All the changes released under the MIT license as the original code.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.DirectoryServices;
using System.Diagnostics;
using System.IO;
using System.Collections;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
using System.Management;
using System.Globalization;
using System.Reflection;
using Microsoft.Win32;

namespace UserAccountGroupCopy
{
    class UAGCMain
    {
        struct CMDArguments
        {
            public string strReferenceUser;
            public string strCheckUser;
            public bool bParseCmdArguments;
        }

        static ManagementObjectCollection funcsysQueryData(string sysQueryString, string sysServerName)
        {

            // [Comment] Connect to the server via WMI
            System.Management.ConnectionOptions objConnOptions = new System.Management.ConnectionOptions();
            string strServerNameforWMI = "\\\\" + sysServerName + "\\root\\cimv2";

            // [DebugLine] Console.WriteLine("Construct WMI scope...");
            System.Management.ManagementScope objManagementScope = new System.Management.ManagementScope(strServerNameforWMI, objConnOptions);

            // [DebugLine] Console.WriteLine("Construct WMI query...");
            System.Management.ObjectQuery objQuery = new System.Management.ObjectQuery(sysQueryString);
            //if (objQuery != null)
            //    Console.WriteLine("objQuery was created successfully");

            // [DebugLine] Console.WriteLine("Construct WMI object searcher...");
            System.Management.ManagementObjectSearcher objSearcher = new System.Management.ManagementObjectSearcher(objManagementScope, objQuery);
            //if (objSearcher != null)
            //    Console.WriteLine("objSearcher was created successfully");

            // [DebugLine] Console.WriteLine("Get WMI data...");

            System.Management.ManagementObjectCollection objReturnCollection = null;

            try
            {
                objReturnCollection = objSearcher.Get();
                return objReturnCollection;
            }
            catch (SystemException ex)
            {
                // [DebugLine] System.Console.WriteLine("{0} exception caught here.", ex.GetType().ToString());
                string strRPCUnavailable = "The RPC server is unavailable. (Exception from HRESULT: 0x800706BA)";
                // [DebugLine] System.Console.WriteLine(ex.Message);
                if (ex.Message == strRPCUnavailable)
                {
                    Console.WriteLine("WMI: Server unavailable");
                }

                // Next line will return an object that is equal to null
                return objReturnCollection;
            }
        }

        static DirectorySearcher funcCreateDSSearcher()
        {
            // [Comment] Get local domain context
            string rootDSE;

            System.DirectoryServices.DirectorySearcher objrootDSESearcher = new System.DirectoryServices.DirectorySearcher();
            rootDSE = objrootDSESearcher.SearchRoot.Path;
            // [DebugLine]Console.WriteLine(rootDSE);

            // [Comment] Construct DirectorySearcher object using rootDSE string
            System.DirectoryServices.DirectoryEntry objrootDSEentry = new System.DirectoryServices.DirectoryEntry(rootDSE);
            System.DirectoryServices.DirectorySearcher objDSSearcher = new System.DirectoryServices.DirectorySearcher(objrootDSEentry);
            // [DebugLine]Console.WriteLine(objDSSearcher.SearchRoot.Path);
            return objDSSearcher;
        }

        static PrincipalContext funcCreatePrincipalContext()
        {
            PrincipalContext newctx = new PrincipalContext(ContextType.Machine);

            try
            {
                //Console.WriteLine("Entering funcCreatePrincipalContext");
                Domain objDomain = Domain.GetComputerDomain();
                string strDomain = objDomain.Name;
                DirectorySearcher tempDS = funcCreateDSSearcher();
                string strDomainRoot = tempDS.SearchRoot.Path.Substring(7);
                // [DebugLine] Console.WriteLine(strDomainRoot);
                // [DebugLine] Console.WriteLine(strDomainRoot);

                newctx = new PrincipalContext(ContextType.Domain,
                                    strDomain,
                                    strDomainRoot);

                // [DebugLine] Console.WriteLine(newctx.ConnectedServer);
                // [DebugLine] Console.WriteLine(newctx.Container);



                //if (strContextType == "Domain")
                //{

                //    PrincipalContext newctx = new PrincipalContext(ContextType.Domain,
                //                                    strDomain,
                //                                    strDomainRoot);
                //    return newctx;
                //}
                //else
                //{
                //    PrincipalContext newctx = new PrincipalContext(ContextType.Machine);
                //    return newctx;
                //}
            }
            catch (Exception ex)
            {
                MethodBase mb1 = MethodBase.GetCurrentMethod();
                funcGetFuncCatchCode(mb1.Name, ex);
            }

            if (newctx.ContextType == ContextType.Machine)
            {
                Exception newex = new Exception("The Active Directory context did not initialize properly.");
                MethodBase mb1 = MethodBase.GetCurrentMethod();
                funcGetFuncCatchCode(mb1.Name, newex);
            }

            return newctx;
        }

        static void funcPrintParameterWarning()
        {
            Console.WriteLine("Parameters must be specified to run UserAccountGroupCopy.");
            Console.WriteLine("Run UserAccountGroupCopy -? to get the parameter syntax.");
        }

        static void funcPrintParameterSyntax()
        {
            Console.WriteLine("UserAccountGroupCopy");
            Console.WriteLine();
            Console.WriteLine("Parameter syntax:");
            Console.WriteLine();
            Console.WriteLine("Use the following parameters:");
            Console.WriteLine("-run                          required parameter");
            Console.WriteLine("-ref:[UserName]               specify reference user");
            Console.WriteLine("-dest:[UserName]              specify destination user");
            Console.WriteLine();
            Console.WriteLine("Examples:");
            Console.WriteLine("UserAccountGroupCopy -run -ref:User1 -dest:User2");
        }

        static void funcLogToEventLog(string strAppName, string strEventMsg, int intEventType)
        {
            string sLog;

            sLog = "Application";

            if (!EventLog.SourceExists(strAppName))
                EventLog.CreateEventSource(strAppName, sLog);

            //EventLog.WriteEntry(strAppName, strEventMsg);
            EventLog.WriteEntry(strAppName, strEventMsg, EventLogEntryType.Information, intEventType);

        } // LogToEventLog

         static void funcGetFuncCatchCode(string strFunctionName, Exception currentex)
        {
            string strCatchCode = "";

            Dictionary<string, string> dCatchTable = new Dictionary<string, string>();
            dCatchTable.Add("funcGetFuncCatchCode", "f0");
            dCatchTable.Add("funcPrintParameterWarning", "f2");
            dCatchTable.Add("funcPrintParameterSyntax", "f3");
            dCatchTable.Add("funcParseCmdArguments", "f4");
            dCatchTable.Add("funcProgramExecution", "f5");
            dCatchTable.Add("funcCreateDSSearcher", "f7");
            dCatchTable.Add("funcCreatePrincipalContext", "f8");
            dCatchTable.Add("funcCheckNameExclusion", "f9");
            dCatchTable.Add("funcMoveDisabledAccounts", "f10");
            dCatchTable.Add("funcFindAccountsToDisable", "f11");
            dCatchTable.Add("funcCheckLastLogin", "f12");
            dCatchTable.Add("funcRemoveUserFromGroup", "f13");
            dCatchTable.Add("funcToEventLog", "f14");
            dCatchTable.Add("funcCheckForFile", "f15");
            dCatchTable.Add("funcCheckForOU", "f16");
            dCatchTable.Add("funcWriteToErrorLog", "f17");

            if (dCatchTable.ContainsKey(strFunctionName))
            {
                strCatchCode = "err" + dCatchTable[strFunctionName] + ": ";
            }

            //[DebugLine] Console.WriteLine(strCatchCode + currentex.GetType().ToString());
            //[DebugLine] Console.WriteLine(strCatchCode + currentex.Message);

            funcWriteToErrorLog(strCatchCode + currentex.GetType().ToString());
            funcWriteToErrorLog(strCatchCode + currentex.Message);

        }

        static CMDArguments funcParseCmdArguments(string[] cmdargs)
        {
            CMDArguments objCMDArguments = new CMDArguments();

            try
            {
                bool bCmdArg1Complete = false;

                if (cmdargs[0] == "-run" & cmdargs.Length > 1)
                {
                    if (cmdargs[1].Contains("-ref:"))
                    {
                        // [DebugLine] Console.WriteLine(cmdargs[1].Substring(5));
                        objCMDArguments.strReferenceUser = cmdargs[1].Substring(5);
                        bCmdArg1Complete = true;

                        if (bCmdArg1Complete & cmdargs.Length > 2)
                        {
                            if (cmdargs[2].Contains("-dest:"))
                            {
                                // [DebugLine] Console.WriteLine(cmdargs[2].Substring(6));
                                objCMDArguments.strCheckUser = cmdargs[2].Substring(6);
                                objCMDArguments.bParseCmdArguments = true;
                            }
                        }
                    }
                }
                else
                {
                    objCMDArguments.bParseCmdArguments = false;
                }
            }
            catch (Exception ex)
            {
                MethodBase mb1 = MethodBase.GetCurrentMethod();
                funcGetFuncCatchCode(mb1.Name, ex);
                objCMDArguments.bParseCmdArguments = false;
            }

            return objCMDArguments;
        }

        static void funcProgramExecution(CMDArguments objCMDArguments2)
        {
            try
            {
                // LogToEventLog("UserAccountGroupCopy", "UserAccountGroupCopy started successfully.", 1401);

                Console.WriteLine();

                //PrincipalContext newctx = funcCreatePrincipalContext();

                //ComputerPrincipal computer1 = new ComputerPrincipal(newctx);
                //ComputerPrincipal computer2 = new ComputerPrincipal(newctx);

                //PrincipalSearcher ps1 = new PrincipalSearcher(computer1);
                //PrincipalSearcher ps2 = new PrincipalSearcher(computer2);

                //computer1.Name = objCMDArguments2.strReferenceComputer;
                //computer2.Name = objCMDArguments2.strCheckComputer;

                //// Tell the PrincipalSearcher what to search for.
                //ps1.QueryFilter = computer1;
                //ps2.QueryFilter = computer2;

                //// Run the query. The query locates users 
                //// that match the supplied user principal object. 
                //Principal newPrincipal1 = ps1.FindOne();
                //Principal newPrincipal2 = ps2.FindOne();

                //if (newPrincipal1 != null | newPrincipal2 != null)
                //{
                //    computer1 = (ComputerPrincipal)newPrincipal1;
                //    computer2 = (ComputerPrincipal)newPrincipal2;

                //    Console.WriteLine("Computers: {0}, {1}", computer1.Name, computer2.Name);

                //    PrincipalSearchResult<Principal> psr1 = computer1.GetGroups();
                //    PrincipalSearchResult<Principal> psr2 = computer2.GetGroups();

                //    Console.WriteLine("Number of groups for {0}: {1}", computer1.Name, psr1.Count<Principal>().ToString());
                //    Console.WriteLine("Number of groups for {0}: {1}", computer2.Name, psr2.Count<Principal>().ToString());

                //    //int intGroupsHigh = 0;
                //    //int intComputer1GroupCount = 0;
                //    //int intComputer2GroupCount = 0;

                //    //intComputer1GroupCount = Int32.Parse(psr1.Count<Principal>().ToString());
                //    //intComputer2GroupCount = Int32.Parse(psr2.Count<Principal>().ToString());

                //    //if (intComputer1GroupCount > intComputer2GroupCount)
                //    //{
                //    //    intGroupsHigh = intComputer1GroupCount;
                //    //}
                //    //else
                //    //{
                //    //    intGroupsHigh = intComputer2GroupCount;
                //    //}

                //    Console.WriteLine();

                //    //List<string> lstOutputLines = new List<string>();

                //    //string strOutputLine = "";
                //    //strOutputLine = "Groups: " + computer1.Name + "\t\t Groups: " + computer2.Name;
                //    //lstOutputLines.Add(strOutputLine);
                //    //strOutputLine = "------- \t\t -------";
                //    //lstOutputLines.Add(strOutputLine);

                //    //foreach (string strConsoleOut in lstOutputLines)
                //    //{
                //    //    Console.WriteLine(strConsoleOut);
                //    //}
                //}
                //else
                //{
                //    Console.WriteLine("One of the computers could not be found.");
                //    Console.WriteLine("Please check the parameters.");
                //}

                // [ Comment] Search filter strings for DirectorySearcher object filter
                string strQueryFilter1 = "";
                string strQueryFilter2 = "";
                string strFilterPrefix = "(&(objectCategory=person)(objectClass=user)(sAMAccountName=";
                string strFilterSuffix = "))";
                string strSourceAccount = "";
                string strDestinationAccount = "";

                strSourceAccount = objCMDArguments2.strReferenceUser;
                strDestinationAccount = objCMDArguments2.strCheckUser;
                strQueryFilter1 = strFilterPrefix + strSourceAccount + strFilterSuffix;
                strQueryFilter2 = strFilterPrefix + strDestinationAccount + strFilterSuffix;

                ManagementObjectCollection oQueryCollection = null;
                oQueryCollection = funcsysQueryData("select * from Win32_ComputerSystem", ".");
                string strDomain = "";

                foreach (ManagementObject oReturn in oQueryCollection)
                {
                    strDomain = oReturn["Domain"].ToString().Trim();
                }

                System.DirectoryServices.DirectorySearcher objAccountObjectSearcher1 = funcCreateDSSearcher();
                System.DirectoryServices.DirectorySearcher objAccountObjectSearcher2 = funcCreateDSSearcher();
                // [DebugLine]Console.WriteLine(objAccountObjectSearcher.SearchRoot.Path);
                string strSearchRoot = objAccountObjectSearcher1.SearchRoot.Path;

                // [Comment] Add filter to DirectorySearcher object
                objAccountObjectSearcher1.Filter = (strQueryFilter1);
                objAccountObjectSearcher2.Filter = (strQueryFilter2);
                objAccountObjectSearcher1.PropertiesToLoad.Add("memberOf");
                objAccountObjectSearcher2.PropertiesToLoad.Add("memberOf");

                // [Comment] Execute query, return results, display values
                System.DirectoryServices.SearchResult objAccountResult1 = objAccountObjectSearcher1.FindOne();
                System.DirectoryServices.DirectoryEntry objAccountResult1DE = new System.DirectoryServices.DirectoryEntry();
                // [DebugLine] Console.WriteLine(objAccountResult2.Path);
                if (objAccountResult1 != null)
                {
                    objAccountResult1DE = new System.DirectoryServices.DirectoryEntry(objAccountResult1.Path);
                }
                // [DebugLine] Console.WriteLine("FindOne: " + objAccountResult1DE.Name);

                System.DirectoryServices.SearchResult objAccountResult2 = objAccountObjectSearcher2.FindOne();
                System.DirectoryServices.DirectoryEntry objAccountResult2DE = new System.DirectoryServices.DirectoryEntry();
                // [DebugLine] Console.WriteLine(objAccountResult2.Path);
                if (objAccountResult2 != null)
                {
                    objAccountResult2DE = new System.DirectoryServices.DirectoryEntry(objAccountResult2.Path);
                }
                // [DebugLine] Console.WriteLine("FindOne: " + objAccountResult2DE.Name);

                if (objAccountResult1 != null & objAccountResult2 != null)
                {

                    string objAccountNameValue;
                    int intStrPosFirst = 3;
                    int intStrPosLast;

                    string strGroupName;

                    intStrPosLast = objAccountResult1DE.Name.Length;
                    objAccountNameValue = objAccountResult1DE.Name.Substring(intStrPosFirst, intStrPosLast - intStrPosFirst);

                    // [DebugLine] Console.WriteLine(objAccountNameValue);

                    if (objAccountResult1DE.Properties["memberOf"].Count > 0)
                    {
                        // [DebugLine] Console.WriteLine("Number of groups: " + objAccountResult1DE.Properties["memberOf"].Count.ToString());
                        Console.WriteLine("Number of groups for {0}: {1}",
                                          objAccountResult1DE.Name.Substring(intStrPosFirst, intStrPosLast - intStrPosFirst),
                                          objAccountResult1DE.Properties["memberOf"].Count.ToString());
                        Console.WriteLine("Number of groups for {0}: {1}",
                                          objAccountResult2DE.Name.Substring(intStrPosFirst, intStrPosLast - intStrPosFirst),
                                          objAccountResult2DE.Properties["memberOf"].Count.ToString());
                        Console.WriteLine();

                        bool isAccount2InGroup = false;

                        for (int propcounter = 0; propcounter < objAccountResult1DE.Properties["memberOf"].Count; propcounter++)
                        {
                            isAccount2InGroup = false;
                            strGroupName = (string)objAccountResult1DE.Properties["memberOf"][propcounter];
                            // [DebugLine] Console.WriteLine(strGroupName);
                            try
                            {
                                DirectoryEntry group = new DirectoryEntry("LDAP://" + strGroupName);
                                // [DebugLine] Console.WriteLine("Number of group members: " + group.Properties["member"].Count.ToString());
                                // [DebugLine] int intmembercounter = 0;

                                foreach (object o in group.Properties["member"])
                                {
                                    // [DebugLine] Console.WriteLine("membercounter: " + intmembercounter.ToString());
                                    // [DebugLine] Console.WriteLine(o.ToString());
                                    // [DebugLine] Console.WriteLine("**Member: " + o.ToString());
                                    // [DebugLine] Console.WriteLine("**Acct2Name: " + objAccountResult2DE.Name);
                                    // [DebugLine] Console.WriteLine("**Acct2Path: " + objAccountResult2DE.Path.Substring(7, objAccountResult2DE.Path.Length-7));
                                    if (objAccountResult2DE.Path.Substring(7, objAccountResult2DE.Path.Length - 7) == o.ToString())
                                    {
                                        isAccount2InGroup = true;
                                    }

                                    // [DebugLine] intmembercounter++;
                                }

                                if (isAccount2InGroup)
                                {
                                    //Console.WriteLine("{0} is a member of this group: {1}", strDestinationAccount, group.Name.Substring(3, group.Name.Length - 3));
                                    Console.WriteLine("{0} group {1}: {2} is a member", strSourceAccount, group.Name.Substring(3, group.Name.Length - 3), strDestinationAccount);
                                    // [DebugLine] Console.WriteLine(group.Path);
                                    // [DebugLine] Console.WriteLine(">>>if: isAccount2InGroup");
                                    //Console.WriteLine();
                                }
                                else
                                {
                                    //Console.WriteLine("{0} is not a member of this group: {1}", strDestinationAccount, group.Name.Substring(3, group.Name.Length - 3));
                                    Console.WriteLine("{0} group {1}: {2} is NOT a member", strSourceAccount, group.Name.Substring(3, group.Name.Length - 3), strDestinationAccount);
                                    // [DebugLine] Console.WriteLine(group.Path);
                                    // [DebugLine] Console.WriteLine(">>>else: isAccount2InGroup");
                                    // [DebugLine] Console.WriteLine();
                                    // [DebugLine] string tmpPath = objAccountResult2DE.Path;
                                    // [DebugLine] Console.WriteLine(tmpPath);
                                    try
                                    {
                                        DirectoryEntry grpEntry = new DirectoryEntry(group.Path);
                                        grpEntry.AuthenticationType = AuthenticationTypes.Secure;
                                        // [DebugLine] Console.WriteLine("grpEntry path: " + grpEntry.Path);
                                        group.Invoke("Add", new object[] { objAccountResult2DE.Path.ToString() });
                                        grpEntry.CommitChanges();
                                        grpEntry.Close();
                                        Console.WriteLine("{0} was added to {1}", strDestinationAccount, group.Name.Substring(3, group.Name.Length - 3));
                                        Console.WriteLine();
                                    }
                                    catch (System.DirectoryServices.DirectoryServicesCOMException e)
                                    {
                                        Console.WriteLine(e.Message.ToString());
                                    }
                                }

                                // [DebugLine] Console.WriteLine(group.Properties["member"].Count.ToString());

                                if (group == null)
                                {
                                    Console.WriteLine("group directoryentry was not created");
                                }
                            }
                            catch (Exception ex)
                            {
                                MethodBase mb1 = MethodBase.GetCurrentMethod();
                                funcGetFuncCatchCode(mb1.Name, ex);
                            }

                        }
                    }
                }
                else
                {
                    Console.WriteLine("One or both of the users could not be found.");
                    Console.WriteLine("Please check the parameters.");
                }
            }
            catch (Exception ex)
            {
                MethodBase mb1 = MethodBase.GetCurrentMethod();
                funcGetFuncCatchCode(mb1.Name, ex);
            }
        }

        static bool funcCheckForFile(string strInputFileName)
        {
            try
            {
                if (System.IO.File.Exists(strInputFileName))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                MethodBase mb1 = MethodBase.GetCurrentMethod();
                funcGetFuncCatchCode(mb1.Name, ex);
                return false;
            }
        }

        static void funcWriteToErrorLog(string strErrorMessage)
        {
            try
            {
                FileStream newFileStream = new FileStream("Err-UserAccountGroupCopy.log", FileMode.Append, FileAccess.Write);
                TextWriter twErrorLog = new StreamWriter(newFileStream);

                DateTime dtNow = DateTime.Now;

                string dtFormat = "MMddyyyy HH:mm:ss";

                twErrorLog.WriteLine("{0} \t {1}", dtNow.ToString(dtFormat), strErrorMessage);

                twErrorLog.Close();
            }
            catch (Exception ex)
            {
                MethodBase mb1 = MethodBase.GetCurrentMethod();
                funcGetFuncCatchCode(mb1.Name, ex);
            }

        }

        static void Main(string[] args)
        {
            try
            {
                if (args.Length == 0)
                {
                    funcPrintParameterWarning();
                }
                else
                {
                    if (args[0] == "-?")
                    {
                        funcPrintParameterSyntax();
                    }
                    else
                    {
                        string[] arrArgs = args;
                        CMDArguments objArgumentsProcessed = funcParseCmdArguments(arrArgs);

                        if (objArgumentsProcessed.bParseCmdArguments)
                        {
                            funcProgramExecution(objArgumentsProcessed);
                        }
                        else
                        {
                            funcPrintParameterWarning();
                        } // check objArgumentsProcessed.bParseCmdArguments
                    } // check args[0] = "-?"
                } // check args.Length == 0
            }
            catch (Exception ex)
            {
                Console.WriteLine("errm0: {0}", ex.Message);
            }
        } // Main
    } // class UAGCMain
} // namespace UserAccountGroupCopy

