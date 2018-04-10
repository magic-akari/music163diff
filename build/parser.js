export const tokenizer = (input) => {
    const splited = input.split(/\s*([\(\)\+\-x])\s*/).filter(i => i !== "");
    if (splited.some(token => !/\d+/.test(token) && !"+-x()".includes(token))) {
        return false;
    }
    return splited;
};
export const parse = (input) => {
    const token_list = tokenizer(input);
    if (token_list === false) {
        return false;
    }
    const seek_token = () => token_list.slice(-1).pop() || false;
    const get_token = () => token_list.pop() || false;
    const get_expr = (token) => {
        const factor = get_factor(token);
        if (factor === false) {
            return false;
        }
        const op = get_op(seek_token());
        if (op === false) {
            return factor;
        }
        else {
            get_token();
            const expr = get_expr(get_token());
            if (expr === false) {
                return false;
            }
            return { op, expr, factor };
        }
    };
    const get_op = (token) => {
        if (token === "+" || token === "-" || token === "x") {
            return token;
        }
        return false;
    };
    const get_factor = (token) => {
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
//# sourceMappingURL=parser.js.map