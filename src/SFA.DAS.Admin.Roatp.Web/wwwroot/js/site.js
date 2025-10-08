const $backLinkOrHome = $('.das-js-back-link');
const backLinkOrHome = function () {
    const backLink = $('<a>')
        .attr({ 'href': '#', 'class': 'govuk-back-link' })
        .text('Back')
        .on('click', function (e) {
            window.history.back();
            e.preventDefault();
        });

    $backLinkOrHome.replaceWith(backLink);

}

if ($backLinkOrHome) {
    backLinkOrHome();
}

