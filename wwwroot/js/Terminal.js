Terminal = {}

Terminal.TerminalScrollToBottom = () => {
    const terminal = document.getElementById("terminal");

    terminal.addEventListener("DOMSubtreeModified", evt => {
        terminal.scrollTop = terminal.scrollHeight;
    })
}
