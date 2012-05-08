namespace au.id.cxd.Math

open System

open TrainingData

/// <summary>
/// A math project is a descriptive type
/// that describes the activities a user can perform within a project.
/// </summary>
module MathProject =

    type DataSource = { DataSource : string }

    /// <summary>
    /// These are the current activities a user can perform
    /// </summary>
    type ProjectActivity = 
            | NoActivity
            | LoadData of DataSource 
            | ReviewData of DataColumn List
            | DualReviewData of DataColumn List
            | MultiReviewData of DataColumn List

    /// <summary>
    /// name for supplied project activity
    /// </summary>
    let activityName act =
        match act with
        | LoadData ds -> "Load Data"
        | ReviewData columns -> "Review Data"
        | DualReviewData columns -> "Review Two Variables"
        | MultiReviewData columns -> "Review Multiple Variables"
        | _ -> "None"

    /// <summary>
    /// Get the data source from the activity
    /// </summary>
    let dataSource act = 
        match act with
            | LoadData ds -> ds.DataSource
            | _ -> ""

    /// <summary>
    /// A collection of actvities that are performed in order of appearance in the list.
    /// </summary>
    type Project = string * ProjectActivity List

    /// <summary>
    /// Get the list of project activities
    /// </summary>
    let projectActivities project = snd project

    /// <summary>
    /// Get the project name
    /// </summary>
    let projectName project = fst project

