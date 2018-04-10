export const tokenizer = (input: string) => {
  const splited = input.split(/\s*([\(\)\+\-x])\s*/).filter(i => i !== "");
  if (splited.some(token => !/\d+/.test(token) && !"+-x()".includes(token))) {
    return false;
  }
  return splited;
};

type OP = "+" | "-" | "x";
type EXPR = AST | string;
type FACTOR = AST | string;

type AST = {
  op: OP;
  expr: EXPR;
  factor: FACTOR;
};

export { AST };

// expr op factor <- expr
// factor          |
//  +             <- op
//  -              |
//  x              |
// (expr)         <-factor
// num             |
export const parse = (input: string) => {
  const token_list = tokenizer(input);
  if (token_list === false) {
    return false;
  }

  const seek_token = () => token_list.slice(-1).pop() || false;

  const get_token = () => token_list.pop() || false;

  // expr op factor <- expr
  // factor          |
  const get_expr = (token: string | false): EXPR | false => {
    const factor = get_factor(token);
    if (factor === false) {
      return false;
    }

    const op = get_op(seek_token());
    // factor <- expr
    if (op === false) {
      return factor;
    } else {
      get_token();

      const expr = get_expr(get_token());
      if (expr === false) {
        return false;
      }

      // expr op factor <- expr
      return { op, expr, factor };
    }
  };

  const get_op = (token: string | false): OP | false => {
    if (token === "+" || token === "-" || token === "x") {
      return token;
    }
    return false;
  };

  // (expr)         <-factor
  // num             |
  const get_factor = (token: string | false): FACTOR | false => {
    if (token === false) {
      return false;
    }
    if (token === ")") {
      const expr = get_expr(get_token());
      if (expr === false || get_token() !== "(") {
        return false;
      }
      return expr;
    }

    if (/^\d+$/.test(token)) {
      return token;
    }
    return false;
  };

  return get_expr(get_token());
};
