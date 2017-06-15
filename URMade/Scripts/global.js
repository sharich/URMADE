; (function ($) {
    $.fn.fixMe = function () {
        return this.each(function () {
            var $this = $(this),
               $t_fixed;
            function init() {
                $this.wrap('<div class="container" />');
                $t_fixed = $this.clone();
                $t_fixed.find("tbody").remove().end().addClass("fixed").insertBefore($this);
                resizeFixed();
            }
            function resizeFixed() {
                $t_fixed.find("th").each(function (index) {
                    $(this).css("width", $this.find("th").eq(index).outerWidth() + "px");
                });
            }
            function scrollFixed() {
                var offset = $(this).scrollTop(),
                tableOffsetTop = $this.offset().top,
                tableOffsetBottom = tableOffsetTop + $this.height() - $this.find("thead").height();
                if (offset < tableOffsetTop || offset > tableOffsetBottom)
                    $t_fixed.hide();
                else if (offset >= tableOffsetTop && offset <= tableOffsetBottom && $t_fixed.is(":hidden"))
                    $t_fixed.show();
            }
            $(window).resize(resizeFixed);
            $(window).scroll(scrollFixed);
            init();
        });
    };
})(jQuery);

$(function () {
    $("table.fix-header").fixMe();

    $("[data-clickable-row]").addClass('clickable-row');

    $("[data-clickable-row]").on('click', function () {
        if ($(this).attr("data-try-new-tab") == "true") {
            window.open($(this).attr('data-clickable-row'), '_blank');
        } else {
            location.href = $(this).attr('data-clickable-row');
        }
    })

    $("[data-clickable-row] a").on('click', function (event) {
        if (event.stopPropagation) {
            event.stopPropagation()
        } else {
            event.cancelBubble = true
        }
    })

});

; (function ($, window, document, undefined) {
    var pluginName = "sortableTable",
		defaults = {
		    fixedHeader: false,
		    fixedColumns: 0
		};

    function Plugin(element, options) {
        this.element = element;

        this.settings = $.extend({}, defaults, options);
        this._defaults = defaults;
        this._name = pluginName;
        this.emptyStringRegex = /^\s*$/;
        this.fralCells = 0;
        this.init();
    }

    // Avoid Plugin.prototype conflicts
    $.extend(Plugin.prototype, {
        init: function () {
            var $table = $(this.element);
            var $headers = $table.find("th");
            var $rows = $table.find("> tbody > tr");
            var that = this;

            this.userSort = null;

            if ($table.is("table") == false) {
                throw "$.sortableTable must be a table element.";
            }

            this.columnHeaders = [];
            this.columnHeadersDictionary = {};
            this.dateColumns = {};

            // Get the initial sort JSON if it exists
            this.initialSort = $table.attr("data-sortable-table");
            if (this.initialSort != "") {
                this.initialSort = JSON.parse(this.initialSort);
            } else {
                this.initialSort = false;
            }

            // Initialize columnHeaders and columnHeadersDictionary.
            $headers.each(function (index) {
                var $th = $(this);
                var column = {
                    columnIndex: index,
                    property: $th.attr("data-sortable-table-column"),
                    prettyName: $th.text()
                };

                $th.attr("data-sortable-table-column-index", index);

                that.columnHeaders.push(column);
                that.columnHeadersDictionary[column.property] = column;
            });

            var dateOrBlankRegex = /^\s*(\d+\/\d+\/\d+)?\s*$/;
            var dateRegex = /^\s*\d+\/\d+\/\d+\s*$/;

            // Identify date columns and save initial order of rows.
            $rows.each(function (index) {
                var $row = $(this);
                $row.attr('data-temp-original-index', index);

                $row.find("> td").each(function () {
                    var $cell = $(this);
                    var columnOfCell = that.getColumn($cell);
                    var text = $cell.text();

                    if (that.dateColumns[columnOfCell.property] != false
						&& text.match(dateOrBlankRegex)) {
                        if (text.match(dateRegex)) {
                            that.dateColumns[columnOfCell.property] = false;
                        }
                    } else {
                        that.dateColumns[columnOfCell.property] = false;
                    }
                });
            });

            if (this.settings.fixedHeader) {
                var fixedTableSettings = {
                    footer: false,
                    cloneHeadToFoot: false
                };


                if (this.settings.fixedColumns > 0) {
                    fixedTableSettings.fixedColumns = this.settings.fixedColumns;
                }
 
                $table.fixedHeaderTable(fixedTableSettings);

                $(".overlay-notice").remove();

                that.fixedTableElement = $table.closest(".fht-table-wrapper");
                that.getFixedColumnRows().each(function (index) {
                    $(this).attr('data-temp-original-index', index);
                });
            } 

            $table.data("redraw", function (customSort) {
                that.redraw(customSort);
            });

            this.redraw();

            this.forAllHeaderCells(function ($th) {
                $th.on('click', function (e) {
                    that.clickHeaderHandler(e, $(this));
                });
            });
        },
        getValueOfProperty: function ($row, property) {
            var index = this.columnHeadersDictionary[property].columnIndex;
            return $($row.children()[index]).text();
        },
        getColumn: function (cell) {
            return this.columnHeaders[$(cell).index()];
        },
        customSortCompare: function ($a, $b, customSort) {
            var sort;
            for (var i = 0; i < customSort.length; i++) {
                var property = this.columnHeaders[customSort[i].column].property;
                sort = this.compareProperties(property, customSort[i].descending, $a, $b);
                if (sort != 0) break;
            }

            return sort;
        },
        compareProperties: function (property, descending, $a, $b) {
            var sortA = this.getValueOfProperty($a, property).toLowerCase();
            var sortB = this.getValueOfProperty($b, property).toLowerCase();

            if (this.dateColumns[property] == true) {
                if (sortA.match(this.emptyStringRegex)) {
                    sortA = Number.MAX_VALUE;
                } else {
                    sortA = (new Date(sortA)).getTime();
                }

                if (sortB.match(this.emptyStringRegex)) {
                    sortB = Number.MAX_VALUE;
                } else {
                    sortB = (new Date(sortB)).getTime();
                }
            }

            if (sortA > sortB) {
                return descending ? 1 : -1;
            } else if (sortA < sortB) {
                return descending ? -1 : 1;
            } else {
                return 0;
            }
        },
        clickHeaderHandler: function (e, $th) {
            var property = $th.attr('data-sortable-table-column');
            var sortByNewColumn = this.userSort == null || this.userSort.property != property;
            var toggleDescending = !sortByNewColumn && this.userSort.property == property && this.userSort.descending;

            this.forAllHeaderCells(function ($th) {
                $th.removeClass("sort-by").removeClass("descending");
            });

            if (sortByNewColumn) {
                this.userSort = { property: property, descending: true };

                this.forHeaderCellsOfProperty(property, function ($th) {
                    $th.addClass("sort-by").addClass("descending");
                });
            } else if (toggleDescending) {
                this.userSort.descending = false;

                this.forHeaderCellsOfProperty(property, function ($th) {
                    $th.addClass("sort-by");
                });
            } else {
                this.userSort = null;
            }

            this.redraw();
        },
        redraw: function (customSort) {
            var sortByData = $(this.element).data("sortBy");
            var passedCustomSort = customSort && customSort.length > 0;
            var usingSortByData = this.userSort == null && sortByData;
            var that = this;

            this.forAllHeaderCells(function ($th) {
                $th.attr('data-custom-sort', "");
            });

            if (passedCustomSort) this.userSort = null;

            if (passedCustomSort || usingSortByData) {
                this.forAllHeaderCells(function ($th) {
                    $th.removeClass("sort-by").removeClass("descending");
                });

                var sort = customSort || sortByData;
                for (var i = 0; i < sort.length; i++) {
                    this.forHeaderCellsOfColumnIndex(sort[i].column, function ($th) {
                        $th.addClass("sort-by");
                        if (sort[i].descending) $th.addClass("descending");
                        if (sort.length > 1) $th.attr('data-custom-sort', i + 1);
                    });
                }
            }

            var $rows = this.getRows();
            var $tbody = $(this.element).find("> tbody");
            $rows = sortRows($rows);
            $rows.each(function () {
                $tbody.append($(this));
            });

            var $fixedRows = this.getFixedColumnRows();
            if ($fixedRows.length > 0) {
                var sortedFixedRows = [];
                $rows.each(function () {
                    var index = $(this).attr('data-temp-original-index');
                    sortedFixedRows.push($fixedRows.filter("[data-temp-original-index='" + index + "']")[0]);
                });

                $tbody = $(this.fixedTableElement)
							.find(".fht-fixed-column .fht-tbody > table > tbody");
                
                for (var i = 0; i < sortedFixedRows.length; i++) {
                    $tbody.append(sortedFixedRows[i]);
                }
            }

            function sortRows($rows) {
                if (!passedCustomSort
                    && that.userSort == null
                    && (!sortByData || sortByData.length == 0)
                    && that.initialSort) {
                    for (var i = 0; i < that.initialSort.length; i++) {
                        that.forHeaderCellsOfProperty(
                            that.initialSort[i].column,
                            function ($th) {
                                $th.addClass("sort-by");
                                if (that.initialSort[i].desc === true) {
                                    $th.addClass("descending");
                                }
                            });
                    }
                }
                
                $rows.sort(function ($a, $b) {
                    $a = $($a);
                    $b = $($b);

                    if (passedCustomSort) {
                        return that.customSortCompare($a, $b, customSort);
                    } else if (that.userSort != null) {
                        return that.compareProperties(
							that.userSort.property,
							that.userSort.descending,
							$a, $b);
                    } else if (sortByData && sortByData.length > 0) {
                        return that.customSortCompare($a, $b, sortByData);
                    } else {
                        var sortA = +$a.attr("data-temp-original-index");
                        var sortB = +$b.attr("data-temp-original-index");

                        return sortA - sortB;
                    }
                });

                return $rows;
            }
        },
        forHeaderCellsOfColumnIndex: function (column, cb) {
            this.forAllHeaderCells(function ($th) {
                if ($th.attr('data-sortable-table-column-index') == "" + column) {
                    cb($th);
                }
            });
        },
        forHeaderCellsOfProperty: function (property, cb) {
            this.forAllHeaderCells(function ($th) {
                if ($th.attr('data-sortable-table-column') == property) {
                    cb($th);
                }
            });
        },
        forAllHeaderCells: function (cb) {
            var callForTh = function ($th) {
                if ($th && $th.length > 0) {
                    $th.each(function () {
                        cb($(this));
                    });
                }
            };

            callForTh(this.getHeaderCells());
            callForTh(this.getFixedColumnHeaderCells());
            callForTh(this.getFixedHeaderCells());
        },
        getFixedColumnHeaderCells: function () {
            return $(this.fixedTableElement).find(".fht-fixed-column .fht-thead th");
        },
        getFixedColumnRows: function () {
            return $(this.fixedTableElement).find(".fht-fixed-column .fht-tbody tr");
        },
        getFixedHeaderCells: function () {
            return $(this.fixedTableElement).find(".fht-fixed-body .fht-thead th");
        },
        getHeaderCells: function () {
            return $(this.element).find("> thead > tr > th");
        },
        getRows: function () {
            return $(this.element).find("> tbody > tr");
        }
    });

    // A really lightweight plugin wrapper around the constructor,
    // preventing against multiple instantiations
    $.fn[pluginName] = function (options) {
        return this.each(function () {
            if (!$.data(this, "plugin_" + pluginName)) {
                $.data(this, "plugin_" +
					pluginName, new Plugin(this, options));
            }
        });
    };

})(jQuery, window, document);

$(function () {

    $("[data-sortable-table]").each(function () {
        var $table = $(this);
        var stickyColumns = parseInt($table.attr("data-sticky-column-count"));

        if (stickyColumns <= 0) {
            $table.sortableTable({ fixedHeader: true, fixedColumns: 1 });
        } else if (isNaN(stickyColumns)) {
            $table.sortableTable();
        } else {
            $table.sortableTable({ fixedHeader: true, fixedColumns: stickyColumns });
        }
    });

})

function loadDotjsTemplates() {
    var deferred = $.Deferred();
    var templateCount = arguments.length;
    var templatesLoaded = 0;
    var loadedTemplates = {};

    var loadTemplate = function (templateName) {
        var id = "template_" + templateName;
        if ($("#" + id).length == 0) {
            $("body").append('<script type="text/template" id="' + id + '"></script>');
        }

        var $container = $("#" + id);
        var prefix = window.location.hostname == "localhost" ? "" : "/URMade";

        $container.load(prefix+ "/Scripts/templates/" + templateName + ".doT.html" + "?" + (new Date()).getTime(), function () {
            loadedTemplates[templateName] = doT.template($container.html());
            templatesLoaded += 1;
            if (templatesLoaded == templateCount) {
                deferred.resolve(loadedTemplates);
            }
        })
    }

    for (var i = 0; i < arguments.length; i++) {
        loadTemplate(arguments[i]);
    }

    return deferred;
}

+function ($) {
    'use strict';

    var isIE = window.navigator.appName == 'Microsoft Internet Explorer';

    // FILEUPLOAD PUBLIC CLASS DEFINITION
    // =================================

    var Fileinput = function (element, options) {
        this.$element = $(element);
        this.$input = this.$element.find(':file');
        if (this.$input.length === 0) {
            return;
        }
        this.name = this.$input.attr('name') || options.name;
        this.$hidden = this.$element.find('input[type=hidden][name="' + this.name + '"]');
        if (this.$hidden.length === 0) {
            this.$hidden = $('<input type="hidden">').insertBefore(this.$input);
        }
        this.$preview = this.$element.find('.fileinput-preview');
        var height = this.$preview.css('height');
        if (this.$preview.css('display') !== 'inline' && height !== '0px' && height !== 'none') {
            this.$preview.css('line-height', height);
        }
        this.original = {
            exists: this.$element.hasClass('fileinput-exists'),
            preview: this.$preview.html(),
            hiddenVal: this.$hidden.val()
        };
        this.listen();
    };
    Fileinput.prototype.listen = function () {
        this.$input.on('change.bs.fileinput', $.proxy(this.change, this));
        $(this.$input[0].form).on('reset.bs.fileinput', $.proxy(this.reset, this));

        this.$element.find('[data-trigger="fileinput"]').on('click.bs.fileinput', $.proxy(this.trigger, this));
        this.$element.find('[data-dismiss="fileinput"]').on('click.bs.fileinput', $.proxy(this.clear, this));
    };
    Fileinput.prototype.change = function (e) {
        var files = e.target.files === undefined ? (e.target && e.target.value ? [{ name: e.target.value.replace(/^.+\\/, '') }] : []) : e.target.files;
        e.stopPropagation();
        if (files.length === 0) {
            this.clear();
            return;
        }
        this.$hidden.val('');
        this.$hidden.attr('name', '');
        this.$input.attr('name', this.name);
        var file = files[0];
        if (this.$preview.length > 0 && (typeof file.type !== 'undefined' ? file.type.match(/^image\/(gif|png|jpeg)$/) : file.name.match(/\.(gif|png|jpe?g)$/i)) && typeof FileReader !== 'undefined') {
            var reader = new FileReader();
            var preview = this.$preview;
            var element = this.$element;

            reader.onload = function (re) {
                var $img = $('<img>');
                $img[0].src = re.target.result;
                files[0].result = re.target.result;
                element.find('.fileinput-filename').text(file.name);

                // if parent has max-height, using `(max-)height: 100%` on child doesn't take padding and border into account
                if (preview.css('max-height') != 'none') {
                    $img.css('max-height', parseInt(preview.css('max-height'), 10) - parseInt(preview.css('padding-top'), 10) - parseInt(preview.css('padding-bottom'), 10) - parseInt(preview.css('border-top'), 10) - parseInt(preview.css('border-bottom'), 10));
                }

                preview.html($img);
                element.addClass('fileinput-exists').removeClass('fileinput-new');
                element.trigger('change.bs.fileinput', files);
            };

            reader.readAsDataURL(file);
        } else {
            this.$element.find('.fileinput-filename').text(file.name);
            this.$preview.text(file.name);
            this.$element.addClass('fileinput-exists').removeClass('fileinput-new');
            this.$element.trigger('change.bs.fileinput');
        }
    };

    Fileinput.prototype.clear = function (e) {
        if (e) {
            e.preventDefault();
        }

        this.$hidden.val('');
        this.$hidden.attr('name', this.name);
        this.$input.attr('name', '');

        // ie8+ doesn't support changing the value of input with type=file so clone instead
        if (isIE) {
            var inputClone = this.$input.clone(true);
            this.$input.after(inputClone);
            this.$input.remove();
            this.$input = inputClone;
        } else {
            this.$input.val('');
        }

        this.$preview.html('');
        this.$element.find('.fileinput-filename').text('');
        this.$element.addClass('fileinput-new').removeClass('fileinput-exists');

        if (e !== undefined) {
            this.$input.trigger('change');
            this.$element.trigger('clear.bs.fileinput');
        }
    };

    Fileinput.prototype.reset = function () {
        this.clear();
        this.$hidden.val(this.original.hiddenVal);
        this.$preview.html(this.original.preview);
        this.$element.find('.fileinput-filename').text('');

        if (this.original.exists) {
            this.$element.addClass('fileinput-exists').removeClass('fileinput-new');
        } else {
            this.$element.addClass('fileinput-new').removeClass('fileinput-exists');
        }

        this.$element.trigger('reset.bs.fileinput');
    };

    Fileinput.prototype.trigger = function (e) {
        this.$input.trigger('click');
        e.preventDefault();
    };


    // FILEUPLOAD PLUGIN DEFINITION
    // ===========================

    function Plugin(options) {
        return this.each(function () {
            var $this = $(this);
            var data = $this.data('bs.fileinput');
            if (!data) {
                $this.data('bs.fileinput', (data = new Fileinput(this, options)));
            }
            if (typeof options == 'string') {
                data[options]();
            }
        });
    }

    var old = $.fn.fileinput;

    $.fn.fileinput = Plugin;
    $.fn.fileinput.Constructor = Fileinput;


    // FILEINPUT NO CONFLICT
    // ====================

    $.fn.fileinput.noConflict = function () {
        $.fn.fileinput = old;
        return this;
    };


    // FILEUPLOAD DATA-API
    // ==================

    $(document).on('click.fileinput.data-api', '[data-provides="fileinput"]', function (e) {
        var $this = $(this);
        if ($this.data('bs.fileinput')) {
            return;
        }
        Plugin.call($this, $this.data());

        var $target = $(e.target).closest('[data-dismiss="fileinput"],[data-trigger="fileinput"]');
        if ($target.length > 0) {
            e.preventDefault();
            $target.trigger('click.bs.fileinput');
        }
    });

}(window.jQuery);

$(function () {
    $("#delete-profile-photo").click(function () {
       $(this).parent().html("");
       $("#DeletePhoto").val("True")
       $("#PhotoUpload").parents(".fileinput").first().show();
   })
})
