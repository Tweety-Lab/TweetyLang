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
    : IDENTIFIER ('::' IDENTIFIER)*
    ;

module_block
    : '{' definition* '}'
    ;

import_statement
    : 'import' module_name ';'
    ;

// Structs
struct_definition
    : modifier* 'struct' IDENTIFIER object_block
    ;

object_block
    : '{' (function_definition | field_declaration)* '}'
    ;

field_declaration
    : type IDENTIFIER ('=' expression)? ';'
    ;

// Functions
function_definition
    : modifier* (type | 'void') IDENTIFIER '(' parameters? ')' (statement_block | ';')
    ;

function_call
    : IDENTIFIER '(' arguments? ')'
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

declaration
    : type IDENTIFIER '=' expression
    ;

assignment
    : IDENTIFIER '=' expression
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
    | IDENTIFIER
    | function_call
    | object_instantiation
    | boolean_literal
    | CHAR_LITERAL
    | STRING_LITERAL
    | '(' expression ')'
    ;

boolean_literal
    : 'true'
    | 'false'
    ;

object_instantiation
    : 'new' IDENTIFIER '(' arguments? ')'
    ;

// Types
parameters
    : parameter (',' parameter)*
    ;

parameter
    : type IDENTIFIER
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
    | IDENTIFIER
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

IDENTIFIER
    : [a-zA-Z] [a-zA-Z0-9_]*
    ;

CHARACTER
    : [a-zA-Z]
    ;

NUMBER : [0-9]+ ;

WS
    : [ \t\r\n]+ -> skip
    ;

MANDATORY_WS
    : [ \t]+
    ;

COMMENT
    : '//' ~[\r\n]* -> skip
    ;
