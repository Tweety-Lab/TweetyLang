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

module_definition
    : 'module' module_name module_body
    ;

module_name
    : identifier ('.' identifier)*
    ;

module_body
    : '{' top_level_declaration* '}'
    ;

import_statement
    : 'import' module_name ';'
    ;

identifier
    : CHARACTER (CHARACTER | DIGIT | '_')*
    ;

function_definition
    : access_modifier (type | 'void') identifier '(' parameters? ')' function_body
    ;

function_body
    : '{' statement* '}'
    ;

function_call
    : identifier '(' arguments? ')'
    ;

arguments
    : expression (',' expression)*
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
access_modifier
    : 'public'
    | 'private'
    ;

// ----------------
// Lexer Rules
// ----------------

CHARACTER
    : [a-zA-Z]
    ;

DIGIT
    : [0-9]
    ;

NUMBER
    : DIGIT+
    ;

WS
    : [ \t\r\n]+ -> skip
    ;

COMMENT
    : '//' ~[\r\n]* -> skip
    ;
