namespace NugetUnicorn.Business2

open System.IO

open System.Collections.Generic
open NugetUnicorn.Dto

type ForbiddenWordsAnalyzer(forbiddenWordsFilePath: string, projectPocos: IEnumerable<ProjectPoco>) = 
    let forbiddenWords = 
        File.ReadAllLines(forbiddenWordsFilePath)
        |> Seq.map (fun x -> x.ToLowerInvariant())
    
    let projectCodeFiles =
        projectPocos
    
    let findForbiddenWordsInProject (project: ProjectPoco) =
        let findForbiddenWordsInLines (itemPath: string) (lines: IEnumerable<System.String>) =
            let getForbiddenWords (index: int) (line: System.String) =
                forbiddenWords
                |> Seq.where (fun word -> line.ToLowerInvariant().Contains(word))
                |> Seq.map (fun word -> (sprintf "file %A line %i has invalid word %A" itemPath index word))

            lines
            |> Seq.mapi (fun index line -> getForbiddenWords index line)

        project.CompilableItems
        |> Seq.map (fun item -> (item, File.ReadAllLines item))
        |> Seq.map (fun (item, allLines) -> findForbiddenWordsInLines item allLines)
        |> Seq.collect (fun x -> x)
        |> Seq.collect (fun x -> x)
    
    member this.Words
        with get() = forbiddenWords
    
    member this.Analyze(): IEnumerable<KeyValuePair<ProjectPoco, IEnumerable<System.String>>> =
        projectPocos
        |> Seq.map (fun poco -> new KeyValuePair<ProjectPoco, IEnumerable<System.String>>(poco, findForbiddenWordsInProject(poco)))
