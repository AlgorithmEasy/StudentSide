Markdown = {}

Markdown.ToHtml = (md, baseUrl) => {
    marked.setOptions({
        baseUrl: baseUrl,
        highlight: (code, lang) => {
            const language = hljs.getLanguage(lang) ? lang : 'plaintext';
            return hljs.highlight(code, { language }).value;
        }
    });
    let html = marked.parse(md);
    html = DOMPurify.sanitize(html);
    return html;
}

window.MathJax = {
    tex: {
        inlineMath: [["$", "$"], ["\\(", "\\)"]]
    },
    options: {
        enableMenu: false
    }
};