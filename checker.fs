//
// Analyzer for simple C programs.  This component performs
// type checking.  The analyzer returns a string denoting
// success or failure. The string "success" if the input 
// program is legal, otherwise the string "type_error: ..." 
// is returned denoting an invalid simple C program.
//
// Modified by:
//   Ronak Chaudhuri
//
// Original author:
//   Prof. Joe Hummel
//   U. of Illinois, Chicago
//   CS 341, Spring 2022
//

namespace compiler

module checker =
  //
  // NOTE: all functions in the module must be indented.
  //


  //
  let private matchToken expected_token (tokens: string list) =
    //
    // if the next token matches the expected token,  
    // keep parsing by returning the rest of the tokens.
    // Otherwise throw an exception because there's a 
    // syntax error, effectively stopping compilation:
    //
    // NOTE: identifier, int_literal and str_literal
    // are special cases because they are followed by
    // the name or literal value. In these cases exact
    // matching will not work, so we match the start 
    // of the token in these cases.
    //
    let next_token = List.head tokens

    if expected_token = "identifier" && next_token.StartsWith("identifier") then
      //
      // next_token starts with identifier, so we have a match:
      //
      List.tail tokens
    elif expected_token = "int_literal" && next_token.StartsWith("int_literal") then
      //
      // next_token starts with int_literal, so we have a match:
      //
      List.tail tokens
    elif expected_token = "str_literal" && next_token.StartsWith("str_literal") then
      //
      // next_token starts with str_literal, so we have a match:
      //
      List.tail tokens
    elif expected_token = "real_literal" && next_token.StartsWith("real_literal") then 
      //
      // next_token starts with real_literal, so we have a match:
      //
      List.tail tokens
    elif expected_token = next_token then  
      List.tail tokens
    else
      failwith ("expecting " + expected_token + ", but found " + next_token)


  //
  // <expr-value> -> identifier
  //               | int_literal
  //               | str_literal
  //               | true
  //               | false
  //
  let rec private expr_value tokens symtab =
    let next_token = List.head tokens
    //
    if next_token = "false" then
      let T2 = matchToken "false" tokens
      (T2, "bool")
    elif next_token = "true" then
      let T2 = matchToken "true" tokens
      (T2, "bool")
    //
    // the others are trickier since we have to look 
    // at the start of the string for a match:
    //
    elif next_token.StartsWith("identifier") then
      let T2 = matchToken "identifier" tokens
      //check variable
      let parts = next_token.Split(':')
      let variableName = parts.[1].Trim()
      let varType = List.find (fun (name, _) -> name = variableName) symtab |> snd
      (T2, varType)
    elif next_token.StartsWith("int_literal") then
      let T2 = matchToken "int_literal" tokens
      (T2, "int")
    elif next_token.StartsWith("str_literal") then
      let T2 = matchToken "str_literal" tokens
      (T2, "str")
    elif next_token.StartsWith("real_literal") then
      let T2 = matchToken "real_literal" tokens
      (T2, "real")
    else
      failwith ("expecting identifier or literal, but found " + next_token)


  //
  // <expr-op> -> +
  //            | -
  //            | *
  //            | /
  //            | ^
  //            | <
  //            | <=
  //            | >
  //            | >=
  //            | ==
  //            | !=
  //
  let rec private expr_op tokens symtab varType = 
    let next_token = List.head tokens
    //
    if next_token = "+"  ||
       next_token = "-"  ||
       next_token = "*"  ||
       next_token = "/"  ||
       next_token = "^"  ||
       next_token = "<"  ||
       next_token = "<=" ||
       next_token = ">"  ||
       next_token = ">=" ||
       next_token = "!=" then
      //
      let T2 = matchToken next_token tokens
      T2
    elif next_token = "==" then
      let (_, varType1) = expr_value (List.tail tokens) symtab
      if varType = "real" || varType1 = "real" then
        printfn "warning: comparing real numbers with == may never be true"
      let T2 = matchToken next_token tokens
      T2
    else
      // error
      failwith ("expecting expression operator, but found " + next_token)


  //
  // <expr> -> <expr-value> <expr-op> <expr-value>
  //         | <expr-value>
  //
  let rec private expr tokens symtab varType= 
    //
    // first we have to match expr-value, since both
    // rules start with this:
    //
    let (T2,varType1) = expr_value tokens symtab
    //
    // now let's see if there's more to the expression:
    //
    let next_token = List.head T2
    //
    if next_token = "+"  ||
       next_token = "-"  ||
       next_token = "*"  ||
       next_token = "/"  ||
       next_token = "^"  ||
       next_token = "<"  ||
       next_token = "<=" ||
       next_token = ">"  ||
       next_token = ">=" ||
       next_token = "==" ||
       next_token = "!=" then
      //
      let T3 = expr_op T2 symtab varType
//Check if first vartype is int or real when operator is +,-,*,/,^
      if varType1 <> "int" && varType1 <> "real" && (next_token = "+" || next_token = "-" || next_token = "*" || next_token = "/" || next_token = "^") then
        failwith ("operator " + next_token + " must involve 'int' or 'real'")
      let (T4,varType2) = expr_value T3 symtab
      //Check if second vartype is int or real when operator is +,-,*,/,^
      if varType2 <> "int" && varType2 <> "real" && (next_token = "+" || next_token = "-" || next_token = "*" || next_token = "/" || next_token = "^") then
        failwith ("operator " + next_token + " must involve 'int' or 'real'")
      //Check type mismatch when operator is comparison
      if (next_token = "<" || next_token = "<=" || next_token = ">" || next_token = ">=" || next_token = "==" || next_token = "!=") && varType1 <> varType2 then
          failwith ("type mismatch '" + varType1 + "' " + next_token + " '" + varType2 + "'")
      T4
    else
      // just expr_value, that's it
      if varType <> varType1 && varType <> "" then
        if varType <> "real" then
          failwith ("cannot assign '" + varType1 + "' to variable of type '" + varType + "'")
        if varType1 <> "int" then
          failwith ("cannot assign '" + varType1 + "' to variable of type '" + varType + "'")
      T2


  //
  // <empty> -> ;
  //
  let rec private empty tokens = 
    let T2 = matchToken ";" tokens
    T2


  //
  // <vardecl> -> int identifier ;
  //
  let rec private vardecl tokens symtab =
    let next_token = List.head tokens
    if next_token = "int" then 
      let T2 = matchToken "int" tokens
      let T3 = matchToken "identifier" T2
      let T4 = matchToken ";" T3
      T4
    else 
      let T2 = matchToken "real" tokens
      let T3 = matchToken "identifier" T2
      let T4 = matchToken ";" T3
      T4


  //
  // <input> -> cin >> identifier ;
  //
  let rec private input tokens symtab = 
    let T2 = matchToken "cin" tokens
    let T3 = matchToken ">>" T2
    let T4 = matchToken "identifier" T3
    let T5 = matchToken ";" T4
    T5


  //
  // <output-value> -> <expr-value>
  //                 | endl
  //
  let rec private output_value tokens symtab = 
    let next_token = List.head tokens
    //
    if next_token = "endl" then
      let T2 = matchToken "endl" tokens
      T2
    else
      let (T2,varType) = expr_value tokens symtab
      T2


  //
  // <output> -> cout << <output-value> ;
  //
  let rec private output tokens symtab = 
    let T2 = matchToken "cout" tokens
    let T3 = matchToken "<<" T2
    let T4 = output_value T3 symtab
    let T5 = matchToken ";" T4
    T5


  //
  // <assignment> -> identifier = <expr> ;
  //
  let rec private assignment tokens symtab = 
    let T2 = matchToken "identifier" tokens
    let T3 = matchToken "=" T2
    let token = List.head tokens
    let parts = token.Split(':')
    let variableName = parts.[1].Trim()
    let varType = List.find (fun (name, _) -> name = variableName) symtab |> snd
    let T4 = expr T3 symtab varType
    let T5 = matchToken ";" T4
    T5


  //
  // <stmt> -> <empty>
  //         | <vardecl>
  //         | <input>
  //         | <output>
  //         | <assignment>
  //         | <ifstmt>
  //
  let rec private stmt tokens symtab = 
    let next_token = List.head tokens
    //
    // use the next token to determine which rule
    // to call; if none match then it's a syntax
    // error:
    //
    if next_token = ";" then
      let T2 = empty tokens
      T2
    elif next_token = "int" || next_token = "real" then
      let T2 = vardecl tokens symtab
      T2
    elif next_token = "cin" then
      let T2 = input tokens symtab
      T2
    elif next_token = "cout" then
      let T2 = output tokens symtab
      T2
    elif next_token.StartsWith("identifier") then
      let T2 = assignment tokens symtab
      T2
    elif next_token = "if" then
      let T2 = ifstmt tokens symtab
      T2
    else
      failwith ("expecting statement, but found " + next_token)
  //
  // <ifstmt> -> if ( <condition> ) <then-part> <else-part>
  //
  and private ifstmt tokens symtab = 
    let T2 = matchToken "if" tokens
    let T3 = matchToken "(" T2
    let T4 = condition T3 symtab
    let T5 = matchToken ")" T4
    let T6 = then_part T5 symtab
    let T7 = else_part T6 symtab
    T7
  //
  // <condition> -> <expr>
  //
  and private condition tokens symtab = 
    let T2 = expr tokens symtab ""
    T2
  //
  // <then-part> -> <stmt>
  //
  and private then_part tokens symtab = 
    let T2 = stmt tokens symtab
    T2
  //
  // <else-part> -> else <stmt>
  //              | EMPTY
  //
  and private else_part tokens symtab = 
    let next_token = List.head tokens
    //
    if next_token = "else" then
      let T2 = matchToken "else" tokens
      let T3 = stmt T2 symtab
      T3
    else
      // EMPTY, do nothing but return tokens back
      tokens


  //
  // <morestmts> -> <stmt> <morestmts>
  //              | EMPTY
  //
  let rec private morestmts tokens symtab = 
    //
    // if the next token denotes the start of a stmt 
    // then process stmt and morestmts, otherwise apply
    // EMPTY
    //
    let next_token = List.head tokens
    //
    if next_token = ";"    ||
       next_token = "int"  ||
       next_token = "real" ||
       next_token = "cin"  ||
       next_token = "cout" ||
       next_token.StartsWith("identifier") ||
       next_token = "if" then
      //
      let T2 = stmt tokens symtab
      let T3 = morestmts T2 symtab
      T3
    else 
      // EMPTY => do nothing, just return tokens back
      tokens


  //
  // <stmts> -> <stmt> <morestmts>
  // 
  let rec private stmts tokens symtab = 
    let T2 = stmt tokens symtab
    let T3 = morestmts T2 symtab
    T3


  //
  // <simpleC> -> void main ( ) { <stmts> } $
  //
  let private simpleC tokens symtab = 
    let T2 = matchToken "void" tokens
    let T3 = matchToken "main" T2
    let T4 = matchToken "(" T3
    let T5 = matchToken ")" T4
    let T6 = matchToken "{" T5
    let T7 = stmts          T6 symtab
    let T8 = matchToken "}" T7
    let T9 = matchToken "$" T8  // $ => EOF, there should be no more tokens
    T9


  //
  // typecheck tokens symboltable
  //
  // Given a list of tokens and a symbol table, type-checks 
  // the program to ensure program's variables and expressions
  // are type-compatible. If the program is valid, returns 
  // the string "success". If the program contains a semantic
  // error or warning, returns a string of the form
  // "type_error: ...".
  //
  let typecheck tokens symboltable = 
    try
      let T2 = simpleC tokens symboltable
      "success"
    with 
      | ex -> "type_error: " + ex.Message

