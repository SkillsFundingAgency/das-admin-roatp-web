const $backLinkOrHome = $('.das-js-back-link');
const backLinkOrHome = function () {

    const referrer = document.referrer;
    const backLink = $('<a>')
        .attr({ 'href': '#', 'class': 'govuk-back-link' })
        .text('Back')
        .on('click', function (e) {
            window.history.back();
            e.preventDefault();
        });

    if (referrer && referrer !== document.location.href) {
        $backLinkOrHome.replaceWith(backLink);
    }
}

if ($backLinkOrHome) {
    backLinkOrHome();
}
