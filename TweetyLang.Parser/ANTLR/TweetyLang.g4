grammar TweetyLang;

// ----------------
// Parser Rules
// ----------------

program
    : top_level_declaration* EOF
    ;

top_level_declaration
    : module_definition
    | import_statement
    | function_definition
    ;

// Modules
module_definition
    : 'module' module_name module_body
    ;

module_name
    : identifier ('::' identifier)*
    ;

module_body
    : '{' top_level_declaration* '}'
    ;

import_statement
    : 'import' module_name ';'
    ;

// Identifiers
identifier
    : CHARACTER (CHARACTER | NUMBER | '_')*
    ;

// Functions
function_definition
    : modifier* (type | 'void') identifier '(' parameters? ')' statement_block
    ;

function_call
    : identifier '(' arguments? ')'
    ;

arguments
    : expression (',' expression)*
    ;

// Statements
statement_block
    : '{' (statement | compound_statement)* '}'
    ;

statement
    : raw_statement ';'
    ;

raw_statement
    : return_statement
    | assignment
    | declaration
    | expression_statement
    ;

compound_statement
    : if_statement
    ;

if_statement
    : 'if' '(' expression ')' statement_block else_block?
    ;

else_block
    : 'else' statement_block
    ;

assignment
    : identifier '=' expression
    ;

declaration
    : type identifier '=' expression
    ;

return_statement
    : 'return' expression?
    ;

expression_statement
    : expression
    ;

// Expressions
expression
    : term (('+' | '-') term)*
    ;

term
    : factor (('*' | '/') factor)*
    ;

factor
    : NUMBER
    | identifier
    | function_call
    | boolean_literal
    | '(' expression ')'
    ;

boolean_literal
    : 'true'
    | 'false'
    ;

// Types
parameters
    : parameter (',' parameter)*
    ;

parameter
    : type identifier
    ;

type
    : raw_type pointer_suffix
    ;

pointer_suffix
    : '*'*
    ;

raw_type
    : 'i32'
    | 'bool'
    ;

// Common
modifier
    : 'export'
    ;

// ----------------
// Lexer Rules
// ----------------

CHARACTER
    : [a-zA-Z]
    ;

NUMBER : [0-9]+ ;

WS
    : [ \t\r\n]+ -> skip
    ;

COMMENT
    : '//' ~[\r\n]* -> skip
    ;
