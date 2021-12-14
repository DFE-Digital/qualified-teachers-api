workspace {

    model {
         # User definitions
        apprBodies = person "Appropriate bodies"
        employers = person "Nurseries, Schools, Academies, Free schools, Independent schools and Colleges"
        organisations = person "Teacher supply agencies, local authorities and training providers"
        heis = person "HEIs - Teacher Trainee Provider"
        teachers = person "Teachers"
        dqtBAU = person "DQT BAU internal team"

        dttp = softwareSystem "Database of Trainee Teachers and Providers" "A registry of Teacher Trainees & Providers"
        dqt = softwareSystem "Database of Qualified Teachers (DQT)" "The DQT is a Customer Relationship Management (CRM) system containing records of all TRN holders in England and Wales"{
            dqtAPI = container "DQT API" "API offering some basic search functionality over the Database of Qualified Teachers (DQT)" "C Sharp"  
            dqtCRM = container "DQT CRM" "A Microsoft CRM Dynamics implementation that contains data on people with Qualified Teacher Status (QTS) and other attributes." "Microsoft Dynamics CRM"
            dqtWebPortal = container "DQT Web Portal" ".Net Portal and Power Apps portal"
            dqtWebService = container "Web Service" "A service that receives organisational data from DSI"
            dqtReportingDB = container "Reporting Database" "SQL Server"
            dqtReportServices = container "Report Services"
            dqtFtpServer = container "FTP server" "Secure FTP Server allowing file exchange across applications"
            dqtEtl = container "Data Interface" "A series of automated jobs to process data"
        }

        dfeSignIn = softwareSystem "DfE Sign In" "Authentication solution for school staff to access DfE online services"
        eapim = softwareSystem "Azure's Enterprise API Management"
        claim = softwareSystem "Claim Additional Payments for Teaching" "Allows eligible teachers to apply for retention payments"
        cpd = softwareSystem "Supports Early Career Framework and National Professional Qualifications"

        apprBodies -> dfeSignIn "signs in"
        employers -> dfeSignIn "signs in"
        organisations -> dfeSignIn "signs in"
        heis -> dfeSignIn "signs in"
        teachers -> dqtWebPortal "signs in"
        dqtBAU -> dqtReportServices "creates reports"

        dfeSignIn -> dqtWebPortal "Redirects to"
        dqtWebPortal -> dqtCRM "persists data to and reads from"
        dqtCRM -> dqtReportingDB "Syncs data to"
        dqtReportingDB -> dqtReportServices "Exports to"
        dqtEtl -> dqtCRM "writes to"
        dqtEtl -> dqtFtpServer "reads from"
        dqtAPI -> dqtCRM "reads and writes to"

        dttp -> dqtFtpServer "Uploads and downloads TRNs and QTSs files"
        eapim -> dqtAPI "approve API users and sets policies"
        claim -> eapim "makes authenticated api requests through"
        cpd -> eapim "makes authenticated api requests through"

    }

    views {

        theme default
    }
}
