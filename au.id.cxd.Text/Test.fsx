
open System



//testing
//0123456
//0 - 123456
//1 - 023456
//2 - 013456
//3 - 012456
//4 - 012356
//5 - 012346
//6 - 012345

let test (word:string) = 

    let len = word.Length


    List.mapi 
            (fun i a -> 
                if (i = 0) then
                    String.Format("{0}:{1}", word.Substring(1, len-1), i)
                else if (i = len-1) then
                    String.Format("{0}:{1}", word.Substring(0, len-1), i)
                else 
                    String.Format("{0}{1}:{2}", word.Substring(0,i), word.Substring(i+1), i)) (word.ToCharArray() |> Array.toList)


let tests = ["testing"; "one"; "a"; "at"; "carnivorous"]
let result1 = List.map test tests

let transtest (word:string) = 
    let chars = (word.ToCharArray() |> Array.toList)
    let len = word.Length
    List.mapi
                (fun i a -> 
                    if (i = len - 1) then
                        ""
                    else 
                        let swap i j (arr:char array) = 
                            let tmp = arr.[i]
                            arr.[i] <- arr.[j]
                            arr.[j] <- tmp
                            new string(arr)
                        swap i (i+1) (word.ToCharArray())) chars

let result2 = List.map transtest tests
