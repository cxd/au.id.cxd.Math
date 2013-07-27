
namespace au.id.cxd.Math.Http.UnitTest
open System
open NUnit.Framework

open au.id.cxd.Math.Http.Filesystem
open au.id.cxd.Math.Http.Cache
open au.id.cxd.Math.Http.Project
open au.id.cxd.Math.Http
open au.id.cxd.Math.Application

[<TestFixture>]
type TestWriteProject() = 
        
        [<Test>]
        member this.TestCreate  () =
            let project = ProjectState.create "test"
            Assert.IsNotNull(project, "Project is null")
            
            
        [<Test>]
        member this.TestLoad() =
            let project2 = ProjectState.loadFromFilesystem "test"
            Assert.IsNotNull(project2, "Could not load project")
            
