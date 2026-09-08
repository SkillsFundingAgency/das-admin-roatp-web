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

function AutoComplete(selectField) {
    this.selectElement = selectField
    this.apiUrl = selectField.dataset.autocompleteUrl || '/registeredProviders'
    this.minLength = Number.parseInt(selectField.dataset.autocompleteMinLength || '2', 10)
    this.mode = selectField.dataset.autocompleteMode || 'provider'
}

AutoComplete.prototype.init = function () {
    this.autoComplete()
}

AutoComplete.prototype.getSuggestions = function (query, updateResults) {
    let results = [];
    let apiUrl = this.apiUrl + "?query=" + encodeURIComponent(query)
    let xhr = new XMLHttpRequest();
    xhr.onreadystatechange = function () {
        if (xhr.readyState === 4) {
            let jsonResponse = JSON.parse(xhr.responseText);
            results = jsonResponse.map(function (result) {
                return result
            });
            updateResults(results);
        }
    }
    xhr.open("GET", apiUrl, true);
    xhr.send();
}


AutoComplete.prototype.onConfirm = function (option) {
    if (!option) {
        return;
    }

    document.getElementById("LegalName").value = option.legalName;
    document.getElementById("Ukprn").value = option.ukprn;
}

function formatAutocompleteResult(result) {
    return result ? [result.legalName, result.ukprn].filter(Boolean).join(' UKPRN: ') : result;
}

AutoComplete.prototype.autoComplete = function () {
    let that = this
    accessibleAutocomplete.enhanceSelectElement({
        selectElement: that.selectElement,
        minLength: that.minLength,
        autoselect: false,
        defaultValue: '',
        displayMenu: 'overlay',
        placeholder: '',
        source: that.getSuggestions.bind(that),
        showAllValues: false,
        confirmOnBlur: false,
        onConfirm: that.onConfirm.bind(that),
        templates: {
            inputValue: formatAutocompleteResult,
            suggestion: formatAutocompleteResult
        }
    });
}

function nodeListForEach(nodes, callback) {
    if (globalThis.NodeList.prototype.forEach) {
        return nodes.forEach(callback)
    }
    for (let i = 0; i < nodes.length; i++) {
        callback.call(globalThis, nodes[i], i, nodes);
    }
}

let autoCompletes = document.querySelectorAll('[data-module="autoComplete"]')

nodeListForEach(autoCompletes, function (autoComplete) {
    new AutoComplete(autoComplete).init()
})

$('.app-autocomplete').each(function () {
    const form = $(this).closest('form');
    const hiddenSelect = document.getElementById(this.id);
    hiddenSelect.setAttribute('aria-hidden', 'true');
    hiddenSelect.setAttribute('tabindex', '-1');
    hiddenSelect.setAttribute('title', 'Hidden select field');

    accessibleAutocomplete.enhanceSelectElement({
        selectElement: this,
        minLength: 3,
        autoselect: false,
        defaultValue: '',
        showAllValues: true,
        displayMenu: 'overlay',
        dropdownArrow: () => '',
        placeholder: $(this).data('placeholder') || '',
        onConfirm: function (opt) {
            const txtInput = document.querySelector('#' + this.id);
            const searchString = opt || txtInput.value;
            const requestedOption = Array.prototype.filter.call(this.selectElement.options, function (option) {
                return (option.textContent || option.innerText) === searchString;
            })[0];
            if (requestedOption) {
                requestedOption.selected = true;
            } else {
                this.selectElement.selectedIndex = 0;
            }
        }
    });

    form.on('submit', function () {
        $('.autocomplete__input').each(function () {
            const that = $(this);
            if (that.val().length === 0) {
                const fieldId = that.attr('id');
                const selectField = $('#' + fieldId + '-select');
                selectField[0].selectedIndex = 0;
            }
        });
    });
});