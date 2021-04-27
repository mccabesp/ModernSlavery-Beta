(function ($) {
    "use strict";

    if ($('.app-high-risk').length == 0)
        return;

    var onConfirm = function (result) {
        var requestedOption = Array.prototype.filter.call(selectElement.options, function (o) {
            return o.innerText === (result && result.name)
        })[0];

        if (requestedOption) {
            requestedOption.selected = true;

            if (document.getElementById("country-line-" + requestedOption.value) == null)
                $('#add-country-button').prop('disabled', false);
        }
    }

    function inputValueTemplate(result) {
        if (typeof result === 'string') {
            return result;
        }
        return result && result.name;
    }

    var suggestionTemplate = function (result) {
        if (typeof result === 'string') {
            return result;
        }
        var path = result && result.path
            ? ' (' + result.path + ')'
            : '';

        return result && result.name + path;
    }

    var hideCountries = function () {

        var rows = $('#selected-countries tbody tr');

        if (rows.length == 0)
            $('#selected-countries').hide();
        else 
            $('#selected-countries').show();

        return false;
    }
    var removeButtonClickHandler = function (e) {
        e.preventDefault();

        var table = $(e.target).parents('table');

        // remove row from document
        $(e.target).parents('tr').remove();

        if (table.find('tr').length >= 0)
        {
            // Re-index the hidden input on each row so the server side isnt confused by the indexing
            table.find('tr').each(function (i, e) {
                var hiddenInput = $(e).find('input[type=hidden]')
                hiddenInput.prop('id', 'CountryReferences_' + i + '_');
                hiddenInput.prop('name', 'CountryReferences[' + i + ']');
            });
        }

        hideCountries();

        return false;
    }

    var addButtonClickHandler = function (e) {
        e.preventDefault();

        if (selectElement.selectedOptions.length == 0 || selectElement.selectedOptions[0].disabled) {
            // nothing selected, show error
            return false;
        }

        var option = selectElement.selectedOptions[0];
        var reference = option.value;
        var rows = $('#selected-countries tbody tr');

        if (rows.length == 0) {
            // show the table
            $('#selected-countries table').show();
        }
        else if (document.getElementById("country-line-" + reference) != null) {
            // already in table
            // show error
            return false;
        }

        var name = option.innerText;
        var index = rows.length;

        var newRow = "<tr id='country-line-" + reference + "' class='govuk-table__row'>" +
            "<td class='govuk-table__cell'>" + name + "</td>" +
            "<td class='govuk-table__cell buttons'>" +
            "<button class='link-button' type='button' name='toRemove' value='" + reference + "'>Remove</button>" +
            "<input id='CountryReferences_" + index + "_' name='CountryReferences[" + index + "]' type='hidden' value='" + reference + "'>" +
            "</td>" +
            "</tr>";

        $('#selected-countries table tbody').append(newRow);

        // clear selected from autocomplete
        // taken from https://github.com/alphagov/accessible-autocomplete/issues/334#issuecomment-621356224 03/12/2020
        var $enhancedElement = $(selectElement).parent().find('input');
        $enhancedElement.val('');
        $(selectElement).val('');

        $enhancedElement.click();
        $enhancedElement.focus();
        $enhancedElement.blur();

        $('#add-country-button').prop('disabled', true);

        hideCountries();
        return false;
    }

    var keydownHandler = function (event) {

        // handle enter key
        if (event.keyCode != 13)
            return;

        event.preventDefault();
        $('#add-country-button').click()
    }

    var keyupHandler = function (event) {
        if (event.target.value.length > 0)
            $('#add-country-button').removeAttr('disabled');
        else
            $('#add-country-button').prop('disabled', true);
    }

    var selectElement = document.getElementById('SelectedCountry');

    openregisterLocationPicker({
        selectElement: selectElement,
        url: '/assets/json/location-autocomplete-graph.json',
        // override suggestion template to match design
        templates: {
            inputValue: inputValueTemplate,
            suggestion: suggestionTemplate
        },
        // override onconfirm to disable the add button until item selected
        onConfirm: onConfirm
    });

    $('#add-country-button')
        .prop('type', 'button')
        .prop('disabled', true);

    $('#selected-countries tr button[name=toRemove]')
        .prop('type', 'button');

    $(document).on("click", '#selected-countries tr button[name=toRemove]', removeButtonClickHandler);

    $('#add-country-button').click(addButtonClickHandler);

    $(document)
        .on('keyup', '#SelectedCountry', keyupHandler)
        .on('keydown', '#SelectedCountry', keydownHandler);

    //This is the only way to disable autocomplete on chrome (see https://stackoverflow.com/questions/12374442/chrome-ignores-autocomplete-off)
    $('#SelectedCountry').attr("autocomplete", Math.random());

    hideCountries();
})(jQuery);