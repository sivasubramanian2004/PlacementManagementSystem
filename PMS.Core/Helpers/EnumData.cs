using System;
using System.Collections.Generic;
using System.Text;

namespace PMS.Core.Helpers;
public enum PlacementStatus
{
        NotPlaced=1,
        Placed=2
}

public enum EducationType
{
    SSLC=1,
    HSC=2,
    Diploma=3,
    Undergraduate=4,
    Postgraduate=5
}

public enum Gender { 

    Male=1,
    Female=2,
    Other=3,

}

public enum UserRole
{
    Student=1,
    Admin=2,
    PlacementOfficer=3,
    Recruiter=4 
}

public enum IndustryType
{
    Software = 1,
    Hardware = 2,
    BPO = 3,
    Marketing = 4,
    HR = 5,
    Electronics = 6,
    Electrical = 7,
    Mechanical = 8,
    Finance = 9,
    Others = 10
}

public enum EmploymentType
{
    FullTime = 1,
    PartTime = 2,
    Internship = 3,
    Contract = 4
}
public enum WorkMode
{
    OnSite = 1,
    Hybrid = 2,
    Remote = 3
}

public enum PlacementDriveStatus
{
    Draft = 1,
    Open = 2,
    Closed = 3,
    Cancelled = 4
}