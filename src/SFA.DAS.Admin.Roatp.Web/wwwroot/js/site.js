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
    this.apiUrl = selectField.getAttribute('data-autocomplete-url') || '/registeredProviders'
    this.minLength = parseInt(selectField.getAttribute('data-autocomplete-min-length') || '2', 10)
    this.mode = selectField.getAttribute('data-autocomplete-mode') || 'provider'
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

    if (this.mode === 'course') {
        document.getElementById("Title").value = option.title ?? option.Title ?? '';
        var level = option.level ?? option.Level;
        document.getElementById("Level").value = (level === undefined || level === null) ? '' : level;
        document.getElementById("LarsCode").value = option.larsCode ?? option.LarsCode ?? '';
        return;
    }

    document.getElementById("LegalName").value = option.legalName;
    document.getElementById("Ukprn").value = option.ukprn;
}

function inputValueTemplate(result) {
    if (result && result.searchTerm) {
        return result.searchTerm;
    }
    return result && [result.legalName, result.ukprn].filter(Boolean).join(' UKPRN: ')
}

function suggestionTemplate(result) {
    if (result && result.searchTerm) {
        return result.searchTerm;
    }
    return result && [result.legalName, result.ukprn].filter(Boolean).join(' UKPRN: ')
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
            inputValue: inputValueTemplate,
            suggestion: suggestionTemplate
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
