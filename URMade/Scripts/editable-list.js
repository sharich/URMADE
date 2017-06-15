$(function () {
    var init = function ($editableListItems) {
        var viewModel = {
            editableLists: []
        };

        var inputsNamed = function (fieldName) {
            var inputSelector = "input[name='" + fieldName + "'], "
                              + "textarea[name='" + fieldName + "'], "
                              + "select[name='" + fieldName + "']";
            return inputSelector;
        }

        var getFieldNames = function ($wrapper) {
            var $inputs = $($wrapper).find("input, textarea, select");
            return _.map($inputs, function (input) {
                return $(input).attr('name');
            });
        }

        var getListItems = function ($containers, fields, propertyName) {
            return _.map($containers, function ($container) {
                $container = $($container);
                var item = {};

                if (propertyName != undefined) {
                    var $allInputs = $container.find("input, textarea, select");
                    var nameMatches = function ($input, name) {
                        var inputName = $input.attr('name');
                        var barePropertyRegex = new RegExp(propertyName + "(\\[\\d*\\])?");
                        var fieldRegex = new RegExp(propertyName + "\\[\\d*\\]\\." + name)


                        return inputName == name
                            || (name == "[]" && inputName.match(barePropertyRegex))
                            || inputName.match(fieldRegex)
                    };


                    _.each(fields, function (fieldName) {
                        var matchingInput = _.find($allInputs, function (input) {
                            return nameMatches($(input), fieldName);
                        });

                        if (matchingInput) {
                            if ($(matchingInput).is("input[type=checkbox]")) {
                                item[fieldName] = $(matchingInput).is(":checked");
                            } else {
                                item[fieldName] = $(matchingInput).val();
                            }
                        }
                    })
                } else {
                    _.each(fields, function (fieldName) {
                        var $input = $container.find(inputsNamed(fieldName)).first();
                        if ($input.is("input[type=checkbox]")) {
                            item[fieldName] = $input.is(":checked");
                        } else {
                            item[fieldName] = $input.val();
                        }
                    });
                }

                return item;
            })
        };

        var blankListItemGenerator = function (fields) {
            return function () {
                var result = {};
                _.each(fields, function (fieldName) {
                    result[fieldName] = "";
                })

                return result;
            }
        }

        var viewForItemRenderer = function (fields, rawTemplate, noun, propertyName) {
            return function (listItem, index) {
                var $result = $("<div data-list-item-index='" + index + "' class='editable-list-item well'>" + rawTemplate + "</div>");
                _.each(fields, function (fieldName) {
                    var $inputs = $result.find(inputsNamed(fieldName));
                    var fieldValue = listItem[fieldName];
                    $inputs.each(function () {
                        var $input = $(this);
                        if ($input.is("input[type=checkbox]")) {
                            $input.attr("checked", !!fieldValue)
                        } else if ($input.is("input")) {
                            $input.attr("value", fieldValue)
                        } else if ($input.is("textarea")) {
                            $input.text(fieldValue);
                        } else if ($input.is("select")) {
                            $input.find("option").removeAttr("selected");
                            $input.find("option[value='" + fieldValue + "']").attr("selected", true);
                        }
                    })

                    //$labelViews = $result.find("[data-view-label='" + fieldName + "']");
                    //if ($labelViews.length > 0) {
                    //    $referenceInput = $($inputs[0]);
                    //    if ($referenceInput.is("select")) {
                    //        if (fieldValue == "") {
                    //            $labelViews.text("");
                    //        } else {
                    //            var $matchingOption = $referenceInput.find("option[value='" + fieldValue + "']");
                    //            if ($matchingOption.length > 0) {
                    //                $labelViews.text($matchingOption.text());
                    //            } else {
                    //                $labelViews.text(fieldValue);
                    //            }
                    //        }
                    //        $referenceInput.find()

                    //    } else {
                    //        $labelViews.text(fieldValue);
                    //    }
                    //}

                    if (fieldName == "[]") {
                        $inputs.attr('name', propertyName + "[" + index + "]");
                    } else {
                        $inputs.attr('name', propertyName + "[" + index + "]." + fieldName);
                    }
                });


                $result.prepend("<div class='remove-editable-list-item btn btn-xs btn-danger'>Remove " +
                    (noun != undefined ? noun : "Item") + "</div>");

                var markup = $result[0].outerHTML;
                return markup;
            }
        }

        var editableLists = _($editableListItems)
            .groupBy(function (item) {
                return $(item).attr('data-editable-list');
            })
            .map(function (initialListItems) {
                var $fieldTemplate = $(initialListItems[0]);

                var propertyName = $fieldTemplate.attr('data-editable-list');
                var fields = getFieldNames($fieldTemplate);
                var listItems = getListItems(initialListItems, fields);
                var getBlankItem = blankListItemGenerator(fields);

                var rawHtml = $fieldTemplate.html();
                var noun = $fieldTemplate.attr('data-editable-list-noun');
                var drawViewForItem = viewForItemRenderer(fields, rawHtml, noun, propertyName);

                var saveChanges = function () {
                    var $items = editableList.findView().find(".editable-list-item");
                    editableList.listItems = getListItems($items, editableList.fields, propertyName);
                }

                var editableList = {
                    propertyName: propertyName,
                    fields: fields,
                    listItems: listItems,
                    drawViewForItem: drawViewForItem,
                    getBlankItem: getBlankItem,
                    $initialElements: $(initialListItems),
                    $firstElement: $fieldTemplate,
                    saveChangesToViewModel: saveChanges,
                    findView: function () {
                        return $("[data-editable-list-view='" + editableList.propertyName + "']");
                    },
                    noun: noun,
                };

                return editableList
            })
            .valueOf();


        var drawList = function (editableList) {
            var markup = ""
            if (editableList.listItems.length > 0) {
                _.each(editableList.listItems, function (item, index) {
                    markup += editableList.drawViewForItem(item, index);
                });
            } else {
                markup += "<div class='well'>No items</div>";
            }

            markup += "<div class='add-item btn-xs btn btn-primary'>Add "
                + (editableList.noun ? editableList.noun : "Item") + "</div>";

            var $view = $("[data-editable-list-view='" + editableList.propertyName + "']");
            $view.html(markup);

            if (editableList.drake == undefined) {
                editableList.drake = dragula([$view[0]]).on('drop', function (el) {
                    editableList.saveChangesToViewModel();
                    drawList(editableList);
                });
            }

            $view.find(".add-item").on('click', function () {
                editableList.saveChangesToViewModel();
                editableList.listItems.push(editableList.getBlankItem());
                drawList(editableList);
            })

            $view.find(".remove-editable-list-item").on('click', function () {
                var index = parseInt($(this).parents(".editable-list-item").first().attr("data-list-item-index"));
                editableList.saveChangesToViewModel();
                editableList.listItems.splice(index, 1);
                drawList(editableList);
            })
        }


        _.each(editableLists, function (editableList) {
            var view = "<div data-editable-list-view='" + editableList.propertyName + "' class='editable-list'></div>";

            editableList.$firstElement.before($(view));
            editableList.$initialElements.remove();

            drawList(editableList);
        })
    }

    var $dataItems = $("[data-editable-list]");
    if ($dataItems.length > 0) {
        init($dataItems);
    }

    $.fn.editableList = function () {
        this.each(function () {
            var $items = $(this).find("[data-editable-list]");
            if ($items.length > 0) {
                init($items);
            }
        });
    };
})