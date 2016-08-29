using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Hammer.Helpers;
using eRoute.Models.eCalendar;

namespace Hammer.Models
{
    public class SMAssessmentModel
    {
        public SMTrainingAssessmentHeader Header { get; set; }

        public List<SMTrainingAssessmentsDetail> DailyWorks { get; set; }
        public List<SMTrainingAssessmentsDetail> Tools { get; set; }
        public List<SMTrainingAssessmentsDetail> Steps { get; set; }
        public SMAssessmentModel GetSMAssessmentModel()
        {
            SMAssessmentModel model = new SMAssessmentModel();
            Header = new SMTrainingAssessmentHeader() { HasTraning = true, Released = true };
            DailyWorks = new List<SMTrainingAssessmentsDetail>();
            Tools = new List<SMTrainingAssessmentsDetail>();
            Steps = new List<SMTrainingAssessmentsDetail>();

            //Get Dailyworking
            foreach (var item in HammerDataProvider.GetDailyWorkingCriteria())
            {
                SMTrainingAssessmentsDetail detail = new SMTrainingAssessmentsDetail();
                detail.CriteriaID = item.CriteriaID;
                detail.CriteriaScore = 1;
                DailyWorks.Add(detail);
            }

            //Tools - 9 cong cu...
            foreach (var item in HammerDataProvider.GetToolsCriteria())
            {
                SMTrainingAssessmentsDetail detail = new SMTrainingAssessmentsDetail();
                detail.CriteriaID = item.CriteriaID;
                detail.CriteriaScore = 1;
                Tools.Add(detail);
            }

            //6 buoc ban hang...
            foreach (var item in HammerDataProvider.GetSaleStepCriteria())
            {
                SMTrainingAssessmentsDetail detail = new SMTrainingAssessmentsDetail();
                detail.CriteriaID = item.CriteriaID;
                detail.CriteriaScore = 1;
                Steps.Add(detail);
            }
            model.Header = Header;
            model.Steps = Steps;
            model.Tools = Tools;
            model.DailyWorks = DailyWorks;
            return model;
        }
        public SMAssessmentModel GetSMAssessmentModel(DateTime assessmentDate, string userID)
        {
            SMAssessmentModel model = new SMAssessmentModel();
            //Get Header
            Header = (from header in HammerDataProvider.Context.SMTrainingAssessmentHeaders
                      where header.AssessmentDate.Date == assessmentDate.Date
                      && header.UserID.Trim() == userID.Trim()
                      select header).SingleOrDefault();

            DailyWorks = (from dw in HammerDataProvider.Context.SMTrainingAssessmentsDetails
                          where dw.AssessmentDate.Date == assessmentDate.Date
                          && dw.UserID == userID &&
                            ((from critDw in HammerDataProvider.Context.SMCriterias
                              where critDw.CriteriaType == Constants.DailyWorks
                                && critDw.Active == true
                              select critDw.CriteriaID).ToList()).Contains(dw.CriteriaID)
                          select dw).ToList();

            Steps = (from dw in HammerDataProvider.Context.SMTrainingAssessmentsDetails
                     where dw.AssessmentDate.Date == assessmentDate.Date
                     && dw.UserID == userID &&
                       ((from critDw in HammerDataProvider.Context.SMCriterias
                         where critDw.CriteriaType == Constants.Steps
                           && critDw.Active == true
                         select critDw.CriteriaID).ToList()).Contains(dw.CriteriaID)
                     select dw).ToList();

            Tools = (from dw in HammerDataProvider.Context.SMTrainingAssessmentsDetails
                     where dw.AssessmentDate.Date == assessmentDate.Date
                     && dw.UserID == userID &&
                       ((from critDw in HammerDataProvider.Context.SMCriterias
                         where critDw.CriteriaType == Constants.Tools
                           && critDw.Active == true
                         select critDw.CriteriaID).ToList()).Contains(dw.CriteriaID)
                     select dw).ToList();

            if (Header == null)
            {
                //Check if @AssessmentDate has training or sumting
                var appointment = (from ap in HammerDataProvider.Context.Appointments
                                   where ap.UserLogin == userID
                                   && ap.ScheduleType == "D" && ap.AllDay == true
                                   && ap.Label.GetValueOrDefault(0) == 3
                                   select ap).SingleOrDefault(x => x.StartDate.Value.Day
                                       == assessmentDate.Day && x.StartDate.Value.Month == assessmentDate.Month
                                       && x.StartDate.Value.Year == assessmentDate.Year);
                if (appointment != null)
                {
                    int distributor = HammerDataProvider.GetSMDistributor(appointment.Employees);
                    Header = new SMTrainingAssessmentHeader()
                    {
                        AssessmentDate = appointment.StartDate.Value.Date,
                        AssessmentFor = appointment.Employees,
                        UserID = userID,
                        DistributorID = distributor.ToString(),
                        AreaID = HammerDataProvider.GetAreaByDistributor(distributor),
                        HasTraning = true
                    };
                }
                else
                {
                    Header = new SMTrainingAssessmentHeader()
                    {
                        AssessmentDate = assessmentDate,
                        UserID = userID,
                        HasTraning = false
                    };
                }
            }

            if (DailyWorks.Count <= 0)
            {
                foreach (var item in HammerDataProvider.GetDailyWorkingCriteria())
                {
                    SMTrainingAssessmentsDetail detail = new SMTrainingAssessmentsDetail();
                    detail.AssessmentDate = assessmentDate;
                    detail.AssessmentFor = Header.AssessmentFor;
                    detail.UserID = userID;
                    detail.CriteriaID = item.CriteriaID;
                    detail.CriteriaScore = 1;
                    DailyWorks.Add(detail);
                }
            }

            if (Steps.Count <= 0)
            {
                foreach (var item in HammerDataProvider.GetSaleStepCriteria())
                {
                    SMTrainingAssessmentsDetail detail = new SMTrainingAssessmentsDetail();
                    detail.AssessmentDate = assessmentDate;
                    detail.AssessmentFor = Header.AssessmentFor;
                    detail.UserID = userID;
                    detail.CriteriaID = item.CriteriaID;
                    detail.CriteriaScore = 1;
                    Steps.Add(detail);
                }
            }

            if (Tools.Count <= 0)
            {
                foreach (var item in HammerDataProvider.GetToolsCriteria())
                {
                    SMTrainingAssessmentsDetail detail = new SMTrainingAssessmentsDetail();
                    detail.AssessmentDate = assessmentDate;
                    detail.AssessmentFor = Header.AssessmentFor;
                    detail.UserID = userID;
                    detail.CriteriaID = item.CriteriaID;
                    detail.CriteriaScore = 1;
                    Tools.Add(detail);
                }
            }
            model.Header = Header;
            model.Steps = Steps;
            model.Tools = Tools;
            model.DailyWorks = DailyWorks;
            return model;
        }
        public SMAssessmentModel GetSMAssessmentModel(DateTime assessmentDate, string userID, int UniqueID)
        {
            SMAssessmentModel model = new SMAssessmentModel();
            //Get Header
            Header = (from header in HammerDataProvider.Context.SMTrainingAssessmentHeaders
                      where header.AssessmentDate.Date == assessmentDate.Date
                      && header.UserID.Trim() == userID.Trim()
                      && header.AssessmentDate.TimeOfDay == assessmentDate.TimeOfDay
                      && header.UniqueID == UniqueID
                      select header).SingleOrDefault();

            DailyWorks = (from dw in HammerDataProvider.Context.SMTrainingAssessmentsDetails
                          where dw.AssessmentDate.Date == assessmentDate.Date
                          && dw.AssessmentDate.TimeOfDay == assessmentDate.TimeOfDay
                          && dw.UniqueID == UniqueID
                          && dw.UserID == userID &&
                            ((from critDw in HammerDataProvider.Context.SMCriterias
                              where critDw.CriteriaType == Constants.DailyWorks
                                && critDw.Active == true
                              select critDw.CriteriaID).ToList()).Contains(dw.CriteriaID)
                          select dw).ToList();

            Steps = (from dw in HammerDataProvider.Context.SMTrainingAssessmentsDetails
                     where dw.AssessmentDate.Date == assessmentDate.Date
                          && dw.AssessmentDate.TimeOfDay == assessmentDate.TimeOfDay
                          && dw.UniqueID == UniqueID
                     && dw.UserID == userID &&
                       ((from critDw in HammerDataProvider.Context.SMCriterias
                         where critDw.CriteriaType == Constants.Steps
                           && critDw.Active == true
                         select critDw.CriteriaID).ToList()).Contains(dw.CriteriaID)
                     select dw).ToList();

            Tools = (from dw in HammerDataProvider.Context.SMTrainingAssessmentsDetails
                     where dw.AssessmentDate.Date == assessmentDate.Date
                     && dw.AssessmentDate.TimeOfDay == assessmentDate.TimeOfDay
                          && dw.UniqueID == UniqueID
                     && dw.UserID == userID &&
                       ((from critDw in HammerDataProvider.Context.SMCriterias
                         where critDw.CriteriaType == Constants.Tools
                           && critDw.Active == true
                         select critDw.CriteriaID).ToList()).Contains(dw.CriteriaID)
                     select dw).ToList();

            if (Header == null)
            {
                //Check if @AssessmentDate has training or sumting
                var appointment = (from ap in HammerDataProvider.Context.Appointments
                                   where ap.UserLogin == userID
                                   && ap.ScheduleType == "D"
                                   && ap.UniqueID == UniqueID
                                   && ap.Label.GetValueOrDefault(0) == 3
                                   select ap).SingleOrDefault(x => x.StartDate.Value.Day
                                       == assessmentDate.Day && x.StartDate.Value.Month == assessmentDate.Month
                                       && x.StartDate.Value.Year == assessmentDate.Year
                                       && x.StartDate.Value.TimeOfDay == assessmentDate.TimeOfDay);
                if (appointment != null)
                {
                    int distributor = HammerDataProvider.GetSMDistributor(appointment.Employees);
                    Header = new SMTrainingAssessmentHeader()
                    {
                        UniqueID = appointment.UniqueID,
                        AssessmentDate = appointment.StartDate.Value,
                        AssessmentFor = appointment.Employees,
                        UserID = userID,
                        DistributorID = distributor.ToString(),
                        AreaID = HammerDataProvider.GetAreaByDistributor(distributor),
                        HasTraning = true,
                        IsDelete = false
                    };
                }
                else
                {
                    Header = new SMTrainingAssessmentHeader()
                    {
                        UniqueID = UniqueID,
                        AssessmentDate = assessmentDate,
                        UserID = userID,
                        HasTraning = false,
                        IsDelete = false
                    };
                }
            }

            if (DailyWorks.Count <= 0)
            {
                foreach (var item in HammerDataProvider.GetDailyWorkingCriteria())
                {
                    SMTrainingAssessmentsDetail detail = new SMTrainingAssessmentsDetail();
                    detail.UniqueID = UniqueID;
                    detail.AssessmentDate = assessmentDate;
                    detail.AssessmentFor = Header.AssessmentFor;
                    detail.UserID = userID;
                    detail.CriteriaID = item.CriteriaID;
                    detail.CriteriaScore = 1;
                    DailyWorks.Add(detail);
                }
            }

            if (Steps.Count <= 0)
            {
                foreach (var item in HammerDataProvider.GetSaleStepCriteria())
                {                    
                        SMTrainingAssessmentsDetail detail = new SMTrainingAssessmentsDetail();
                        detail.UniqueID = UniqueID;
                        detail.AssessmentDate = assessmentDate;
                        detail.AssessmentFor = Header.AssessmentFor;
                        detail.UserID = userID;
                        detail.CriteriaID = item.CriteriaID;
                        detail.CriteriaScore = 1;
                        Steps.Add(detail);                    
                }
            }

            if (Tools.Count <= 0)
            {
                foreach (var item in HammerDataProvider.GetToolsCriteria())
                {                    
                        SMTrainingAssessmentsDetail detail = new SMTrainingAssessmentsDetail();
                        detail.UniqueID = UniqueID;
                        detail.AssessmentDate = assessmentDate;
                        detail.AssessmentFor = Header.AssessmentFor;
                        detail.UserID = userID;
                        detail.CriteriaID = item.CriteriaID;
                        detail.CriteriaScore = 1;
                        Tools.Add(detail);                    
                }
            }
            model.Header = Header;
            model.Steps = Steps;
            model.Tools = Tools;
            model.DailyWorks = DailyWorks;
            return model;
        }
    }

    public class AssessmentModel
    {
        public TrainingAssessmentHeader Header { get; set; }

        public List<TrainingAssessmentsDetail> TrainingProcess { get; set; }
        //public List<TrainingAssessmentsDetail> TrainingQuality { get; set; }
        //public List<TrainingAssessmentsDetail> Tools { get; set; }
        public List<TrainingAssessmentsDetail> UpdateAndArchive { get; set; }
        // SM
        public List<TrainingAssessmentsDetail> DailyWorks { get; set; }
        public List<TrainingAssessmentsDetail> ToolsSM { get; set; }
        public List<TrainingAssessmentsDetail> Steps { get; set; }
        //

        public AssessmentModel()
        {

            Header = new TrainingAssessmentHeader() { HasTraning = true };
            TrainingProcess = new List<TrainingAssessmentsDetail>();
            DailyWorks = new List<TrainingAssessmentsDetail>();
            ToolsSM = new List<TrainingAssessmentsDetail>();
            Steps = new List<TrainingAssessmentsDetail>();
            UpdateAndArchive = new List<TrainingAssessmentsDetail>();

            //Training Process
            foreach (var item in HammerDataProvider.GetTrainingProcessCriterias())
            {
                TrainingAssessmentsDetail detail = new TrainingAssessmentsDetail();
                detail.CriteriaID = item.CriteriaID;
                detail.CriteriaScore = 1;
                TrainingProcess.Add(detail);
            }
            //SM
            #region SM
            //Get Dailyworking
            foreach (var item in HammerDataProvider.SSGetDailyWorkingCriteria())
            {
                TrainingAssessmentsDetail detail = new TrainingAssessmentsDetail();
                detail.CriteriaID = item.CriteriaID;
                detail.CriteriaScore = 1;
                DailyWorks.Add(detail);
            }

            //Tools - 9 cong cu...
            foreach (var item in HammerDataProvider.SSGetToolsCriteria())
            {
                TrainingAssessmentsDetail detail = new TrainingAssessmentsDetail();
                detail.CriteriaID = item.CriteriaID;
                detail.CriteriaScore = 1;
                ToolsSM.Add(detail);
            }

            //6 buoc ban hang...
            foreach (var item in HammerDataProvider.SSGetSaleStepCriteria())
            {
                TrainingAssessmentsDetail detail = new TrainingAssessmentsDetail();
                detail.CriteriaID = item.CriteriaID;
                detail.CriteriaScore = 1;
                Steps.Add(detail);
            }
            #endregion
            //endSM  
            //Update and Archive
            foreach (var item in HammerDataProvider.GetUpdateArchiveCriterias())
            {
                TrainingAssessmentsDetail detail = new TrainingAssessmentsDetail();
                detail.CriteriaID = item.CriteriaID;
                detail.CriteriaScore = 1;
                UpdateAndArchive.Add(detail);
            }
        }
        public AssessmentModel(DateTime assessmentDate, string userID)
        {
            //Get Header
            Header = (from header in HammerDataProvider.Context.TrainingAssessmentHeaders
                      where header.AssessmentDate.Date == assessmentDate.Date
                      && header.UserID.Trim() == userID.Trim()
                      select header).SingleOrDefault();

            TrainingProcess = (from dw in HammerDataProvider.Context.TrainingAssessmentsDetails
                               where dw.AssessmentDate.Date == assessmentDate.Date
                               && dw.UserID == userID &&
                                 ((from critDw in HammerDataProvider.Context.Criterias
                                   where critDw.CriteriaType == Constants.TrainingProcess
                                    && critDw.Active == true
                                   select critDw.CriteriaID).ToList()).Contains(dw.CriteriaID)
                               select dw).ToList();

            UpdateAndArchive = (from dw in HammerDataProvider.Context.TrainingAssessmentsDetails
                                where dw.AssessmentDate.Date == assessmentDate.Date
                                && dw.UserID == userID &&
                                  ((from critDw in HammerDataProvider.Context.Criterias
                                    where critDw.CriteriaType == Constants.UpdateArchive
                                      && critDw.Active == true
                                    select critDw.CriteriaID).ToList()).Contains(dw.CriteriaID)
                                select dw).ToList();
            //SM
            #region SM
            DailyWorks = (from dw in HammerDataProvider.Context.TrainingAssessmentsDetails
                          where dw.AssessmentDate.Date == assessmentDate.Date
                          && dw.UserID == userID &&
                            ((from critDw in HammerDataProvider.Context.Criterias
                              where critDw.CriteriaType == Constants.DailyWorks
                                && critDw.Active == true
                              select critDw.CriteriaID).ToList()).Contains(dw.CriteriaID)
                          select dw).ToList();

            Steps = (from dw in HammerDataProvider.Context.TrainingAssessmentsDetails
                     where dw.AssessmentDate.Date == assessmentDate.Date
                     && dw.UserID == userID &&
                       ((from critDw in HammerDataProvider.Context.Criterias
                         where critDw.CriteriaType == Constants.Steps
                           && critDw.Active == true
                         select critDw.CriteriaID).ToList()).Contains(dw.CriteriaID)
                     select dw).ToList();

            ToolsSM = (from dw in HammerDataProvider.Context.TrainingAssessmentsDetails
                       where dw.AssessmentDate.Date == assessmentDate.Date
                       && dw.UserID == userID &&
                         ((from critDw in HammerDataProvider.Context.Criterias
                           where critDw.CriteriaType == Constants.Tools
                             && critDw.Active == true
                           select critDw.CriteriaID).ToList()).Contains(dw.CriteriaID)
                       select dw).ToList();
            #endregion

            if (Header == null)
            {
                //Check if @AssessmentDate has training or sumting
                var appointment = (from ap in HammerDataProvider.Context.Appointments
                                   where ap.UserLogin == userID
                                   && ap.ScheduleType == "D" && ap.AllDay == true
                                   && ap.Label.GetValueOrDefault(0) == 3
                                   select ap).SingleOrDefault(x => x.StartDate.Value.Day
                                       == assessmentDate.Day && x.StartDate.Value.Month == assessmentDate.Month
                                       && x.StartDate.Value.Year == assessmentDate.Year);
                if (appointment != null)
                {
                    int distributor = HammerDataProvider.GetWWDistributor(appointment);

                    Header = new TrainingAssessmentHeader()
                    {
                        AssessmentDate = appointment.StartDate.Value,
                        AssessmentFor = appointment.Employees,
                        UserID = userID,
                        DistributorID = distributor.ToString(),
                        AreaID = HammerDataProvider.GetAreaByDistributor(distributor),
                        HasTraning = true,
                        IsDelete = false
                    };
                }
                else
                {
                    Header = new TrainingAssessmentHeader()
                    {
                        AssessmentDate = assessmentDate,
                        UserID = userID,
                        HasTraning = false,
                        IsDelete = false
                    };
                }
            }


            if (UpdateAndArchive.Count <= 0)
            {
                foreach (var item in HammerDataProvider.GetUpdateArchiveCriterias())
                {
                    TrainingAssessmentsDetail detail = new TrainingAssessmentsDetail();
                    detail.AssessmentDate = assessmentDate;
                    detail.AssessmentFor = Header.AssessmentFor;
                    detail.UserID = userID;
                    detail.CriteriaID = item.CriteriaID;
                    detail.CriteriaScore = 1;
                    UpdateAndArchive.Add(detail);
                }
            }

            if (TrainingProcess.Count <= 0)
            {
                foreach (var item in HammerDataProvider.GetTrainingProcessCriterias())
                {
                    TrainingAssessmentsDetail detail = new TrainingAssessmentsDetail();
                    detail.AssessmentDate = assessmentDate;
                    detail.AssessmentFor = Header.AssessmentFor;
                    detail.UserID = userID;
                    detail.CriteriaID = item.CriteriaID;
                    detail.CriteriaScore = 1;
                    TrainingProcess.Add(detail);
                }
            }
            //SM
            if (DailyWorks.Count <= 0)
            {
                foreach (var item in HammerDataProvider.SSGetDailyWorkingCriteria())
                {
                    TrainingAssessmentsDetail detail = new TrainingAssessmentsDetail();
                    detail.AssessmentDate = assessmentDate;
                    detail.AssessmentFor = Header.AssessmentFor;
                    detail.UserID = userID;
                    detail.CriteriaID = item.CriteriaID;
                    detail.CriteriaScore = 1;
                    DailyWorks.Add(detail);
                }
            }

            if (Steps.Count <= 0)
            {
                foreach (var item in HammerDataProvider.SSGetSaleStepCriteria())
                {
                    TrainingAssessmentsDetail detail = new TrainingAssessmentsDetail();
                    detail.AssessmentDate = assessmentDate;
                    detail.AssessmentFor = Header.AssessmentFor;
                    detail.UserID = userID;
                    detail.CriteriaID = item.CriteriaID;
                    detail.CriteriaScore = 1;
                    Steps.Add(detail);
                }
            }

            if (ToolsSM.Count <= 0)
            {
                foreach (var item in HammerDataProvider.SSGetToolsCriteria())
                {
                    TrainingAssessmentsDetail detail = new TrainingAssessmentsDetail();
                    detail.AssessmentDate = assessmentDate;
                    detail.AssessmentFor = Header.AssessmentFor;
                    detail.UserID = userID;
                    detail.CriteriaID = item.CriteriaID;
                    detail.CriteriaScore = 1;
                    ToolsSM.Add(detail);
                }
            }
            //SM
        }
        public AssessmentModel(DateTime assessmentDate, string userID, int UniqueID)
        {

            //Get Header
            Header = (from header in HammerDataProvider.Context.TrainingAssessmentHeaders
                      where header.AssessmentDate.Date == assessmentDate.Date
                      //&& header.AssessmentDate.TimeOfDay == assessmentDate.TimeOfDay
                      && header.UniqueID == UniqueID
                      && header.UserID.Trim() == userID.Trim()
                      select header).SingleOrDefault();
            UpdateAndArchive = (from dw in HammerDataProvider.Context.TrainingAssessmentsDetails
                                where dw.AssessmentDate.Date == assessmentDate.Date && dw.UniqueID == UniqueID
                                && dw.UserID == userID && 
                                  ((from critDw in HammerDataProvider.Context.Criterias
                                    where critDw.CriteriaType == Constants.UpdateArchive
                                      && critDw.Active == true
                                    select critDw.CriteriaID).ToList()).Contains(dw.CriteriaID)
                                select dw).ToList();

            TrainingProcess = (from dw in HammerDataProvider.Context.TrainingAssessmentsDetails
                               where dw.AssessmentDate.Date == assessmentDate.Date
                               && dw.AssessmentDate.TimeOfDay == assessmentDate.TimeOfDay
                               && dw.UniqueID == UniqueID
                               && dw.UserID == userID &&
                                 ((from critDw in HammerDataProvider.Context.Criterias
                                   where critDw.CriteriaType == Constants.TrainingProcess
                                     && critDw.Active == true
                                   select critDw.CriteriaID).ToList()).Contains(dw.CriteriaID)
                               select dw).ToList();
            //SM
            #region SM
            DailyWorks = (from dw in HammerDataProvider.Context.TrainingAssessmentsDetails
                          where dw.AssessmentDate.Date == assessmentDate.Date && dw.UniqueID == UniqueID
                          && dw.UserID == userID &&
                            ((from critDw in HammerDataProvider.Context.Criterias
                              where critDw.CriteriaType == Constants.DailyWorks
                                && critDw.Active == true
                              select critDw.CriteriaID).ToList()).Contains(dw.CriteriaID)
                          select dw).ToList();
            ToolsSM = (from dw in HammerDataProvider.Context.TrainingAssessmentsDetails
                       where dw.AssessmentDate.Date == assessmentDate.Date && dw.UniqueID == UniqueID
                       && dw.UserID == userID &&
                         ((from critDw in HammerDataProvider.Context.Criterias
                           where critDw.CriteriaType == Constants.ToolsSM
                             && critDw.Active == true
                           select critDw.CriteriaID).ToList()).Contains(dw.CriteriaID)
                       select dw).ToList();
            Steps = (from dw in HammerDataProvider.Context.TrainingAssessmentsDetails
                     where dw.AssessmentDate.Date == assessmentDate.Date && dw.UniqueID == UniqueID
                     && dw.UserID == userID &&
                       ((from critDw in HammerDataProvider.Context.Criterias
                         where critDw.CriteriaType == Constants.Steps
                           && critDw.Active == true
                         select critDw.CriteriaID).ToList()).Contains(dw.CriteriaID)
                     select dw).ToList();

            #endregion
            string EmployeTem = string.Empty;
            string SMTem = string.Empty;

            if (Header == null)
            {
                //Check if @AssessmentDate has training or sumting
                var appointment = (from ap in HammerDataProvider.Context.Appointments
                                   where ap.UserLogin == userID
                                   && ap.UniqueID == UniqueID
                                   && ap.ScheduleType == "D"
                                   && ap.Label.GetValueOrDefault(0) == 3
                                   select ap).SingleOrDefault(x => x.StartDate.Value.Day
                                       == assessmentDate.Day && x.StartDate.Value.Month == assessmentDate.Month
                                       && x.StartDate.Value.Year == assessmentDate.Year
                                       && x.StartDate.Value.TimeOfDay == assessmentDate.TimeOfDay);

                if (appointment != null)
                {
                    int distributor = HammerDataProvider.GetWWDistributor(appointment);
                    EmployeeModel SS = HammerDataProvider.GetSSAssmentforUnique(appointment);
                    if (SS != null)
                    {
                        EmployeTem = SS.EmployeeID;
                        SMTem = appointment.Employees;
                    }
                    else
                    {
                        EmployeTem = null;
                        SMTem = appointment.Employees;
                    }
                    Header = new TrainingAssessmentHeader()
                    {
                        UniqueID = appointment.UniqueID,
                        AssessmentDate = appointment.StartDate.Value,
                        AssessmentFor = EmployeTem,
                        SM = SMTem,
                        UserID = userID,
                        DistributorID = distributor.ToString(),
                        AreaID = HammerDataProvider.GetAreaByDistributor(distributor),
                        HasTraning = true
                    };
                }
                else
                {
                    Header = new TrainingAssessmentHeader()
                    {
                        AssessmentDate = assessmentDate,
                        UserID = userID,
                        HasTraning = false
                    };
                }
            }
            if (UpdateAndArchive.Count <= 0)
            {
                foreach (var item in HammerDataProvider.GetUpdateArchiveCriterias())
                {
                    TrainingAssessmentsDetail detail = new TrainingAssessmentsDetail();
                    detail.AssessmentDate = assessmentDate;
                    detail.AssessmentFor = Header.AssessmentFor;
                    detail.UserID = userID;
                    detail.CriteriaID = item.CriteriaID;
                    detail.CriteriaScore = 1;
                    UpdateAndArchive.Add(detail);
                }
            }
            if (TrainingProcess.Count <= 0)
            {
                foreach (var item in HammerDataProvider.GetTrainingProcessCriterias())
                {
                    TrainingAssessmentsDetail detail = new TrainingAssessmentsDetail();
                    detail.UniqueID = Header.UniqueID.Value;
                    detail.AssessmentDate = assessmentDate;
                    detail.AssessmentFor = EmployeTem;
                    detail.SM = SMTem;
                    detail.UserID = userID;
                    detail.CriteriaID = item.CriteriaID;
                    if (Header.AssessmentFor != null)
                    {
                        detail.CriteriaScore = 1;
                    }
                    else
                    {
                        detail.CriteriaScore = 0;
                    }
                    TrainingProcess.Add(detail);
                }
            }
            //SM
            if (DailyWorks.Count <= 0)
            {
                foreach (var item in HammerDataProvider.SSGetDailyWorkingCriteria())
                {
                    TrainingAssessmentsDetail detail = new TrainingAssessmentsDetail();
                    detail.AssessmentDate = assessmentDate;
                    detail.AssessmentFor = Header.AssessmentFor;
                    detail.UserID = userID;
                    detail.CriteriaID = item.CriteriaID;
                    detail.CriteriaScore = 1;
                    DailyWorks.Add(detail);
                }
            }
            if (ToolsSM.Count <= 0)
            {
                foreach (var item in HammerDataProvider.SSGetToolsCriteria())
                {
                    TrainingAssessmentsDetail detail = new TrainingAssessmentsDetail();
                    detail.AssessmentDate = assessmentDate;
                    detail.AssessmentFor = Header.AssessmentFor;
                    detail.UserID = userID;
                    detail.CriteriaID = item.CriteriaID;
                    detail.CriteriaScore = 1;
                    ToolsSM.Add(detail);
                }
            }
            if (Steps.Count <= 0)
            {
                foreach (var item in HammerDataProvider.SSGetSaleStepCriteria())
                {
                    TrainingAssessmentsDetail detail = new TrainingAssessmentsDetail();
                    detail.AssessmentDate = assessmentDate;
                    detail.AssessmentFor = Header.AssessmentFor;
                    detail.UserID = userID;
                    detail.CriteriaID = item.CriteriaID;
                    detail.CriteriaScore = 1;
                    Steps.Add(detail);
                }
            }
            //SM
        }
    }
    public class ComboDateAssessmentModel
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int UniqueID { get; set; }
    }
    public class NoAssessmentModel
    {
        public NoTrainingAssessment Header { get; set; }
        public bool HasTraining { get; set; }
        public bool AllowChange { get; set; }

        public NoAssessmentModel()
        {
            Header = new NoTrainingAssessment();
            HasTraining = false;
            AllowChange = false;
        }

        public NoAssessmentModel(DateTime assessmentDate, string employeeID)
        {
            Header = HammerDataProvider.GetNoTrainingAssessment(assessmentDate, employeeID);
            HasTraining = false;
            AllowChange = false;
        }
        public NoAssessmentModel(DateTime assessmentDate, string employeeID, string UniqueID)
        {
            Header = HammerDataProvider.GetNoTrainingAssessment(assessmentDate, employeeID, UniqueID);           
            HasTraining = false;
            AllowChange = false;
        }
    }
}