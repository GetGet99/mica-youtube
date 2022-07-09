(
    function() {
        let html = document.getElementsByTagName('html')[0];
        html.setAttribute('style', `${html.getAttribute('style')} background: transparent !important;`);
        let masthead = document.getElementById('masthead');
        masthead.style.background = 'transparent';
    }
)()