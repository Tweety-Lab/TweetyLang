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
    ;

// Modules
module_definition
    : 'module' module_name module_block
    ;

module_name
    : identifier ('::' identifier)*
    ;

module_block
    : '{' definition* '}'
    ;

import_statement
    : 'import' module_name ';'
    ;

// Identifiers
identifier
    : CHARACTER (CHARACTER | NUMBER | '_')*
    ;

// Structs
struct_definition
    : modifier* 'struct' identifier object_block
    ;

object_block
    : '{' (function_definition | field_declaration)* '}'
    ;

field_declaration
    : type identifier ('=' expression)? ';'
    ;

// Functions
function_definition
    : modifier* (type | 'void') identifier '(' parameters? ')' (statement_block | ';')
    ;

function_call
    : identifier '(' arguments? ')'
    ;

arguments
    : expression (',' expression)*
    ;

definition 
    : struct_definition
    | function_definition
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
    | CHAR_LITERAL
    | STRING_LITERAL
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
    | 'char'
    ;

// Common
modifier
    : 'export'
    | 'extern'
    ;


// ----------------
// Lexer Rules
// ----------------

CHAR_LITERAL
    : '\'' ( ESCAPE_SEQUENCE | ~['\\\r\n] ) '\''
    ;

STRING_LITERAL
    : '"' ( ESCAPE_SEQUENCE | ~["\\\r\n] )* '"'
    ;

ESCAPE_SEQUENCE
    : '\\' .  // matches a backslash followed by any single character
    ;

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
