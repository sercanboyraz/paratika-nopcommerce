(function ($, ssCore) {
    $(document).ready(function () {

        var closeMenuSelector = '.close-menu > span';
        var flyoutCartScrollbarSelector = '#flyout-cart .items';
        var overlayElementSelector = '.overlayOffCanvas';

        var dependencies = [
            {
                module: "header",
                dependencies: ["attachDetach", "overlay", "perfectScrollbar"]
            },
            {
                module: "menu",
                dependencies: ["perfectScrollbar"]
            }
        ];

        var themeSettings = {
            productPageThumbs: true,
            attachDetach: {
                blocks: [
                    {
                        multipleContentElements: $('.product-variant-list').length,
                        parentHolder: '.product-variant-line',
                        traversingFunction: 'find',
                        content: '.qty-wrapper',
                        elementToAttach: '.quantity-container'
                    },
                    {
                        multipleContentElements: $('.blog-posts').length,
                        parentHolder: '.blog-posts .post',
                        traversingFunction: 'find',
                        content: '.post-relations',
                        elementToAttach: '.blog-details'
                    },
                    {
                        multipleContentElements: $('.product-variant-list').length,
                        parentHolder: '.product-variant-line',
                        traversingFunction: 'find',
                        content: '.min-qty-notification',
                        elementToAttach: '.quantity-container'
                    }
                ]
            },
            menu: {
                closeMenuSelector: closeMenuSelector,
                sublistIndent: {
                    enabled: true,
                }
            },
            goToTop: {
                animation: {
                    type: 'slide',      // Fade, slide, none
                    speed: 500            // Animation in speed (ms)
                }
            },
            productQuantity: {
                quantityInputSelector: '.qty-input',
                incrementSelectors: '.increase',
                decrementSelectors: '.decrease' 
            },
            flyoutCart: {
                flyoutCartSelector: '#flyout-cart',
                flyoutCartScrollbarSelector: flyoutCartScrollbarSelector,
                removeItemSelector: '#flyout-cart .remove-item'
            },
            equalizer: {
                blocks: [
                     {
                         selector: '.cart tbody tr',
                         property: 'height',
                         measurementFunction: 'outerHeight',
                         includeMargin: true
                     },
                     {
                         selector: '.compare-section .item > div',
                         property: 'height',
                         measurementFunction: 'innerHeight'
                     },
                     {
                         selector: '.news-list-homepage .news-body',
                         property: 'height',
                         measurementFunction: 'innerHeight'
                     },
                     {
                         selector: '.product-list .product-item > div',
                         property: 'height',
                         measurementFunction: 'innerHeight'
                     },
                     {
                         selector: '.customer-blocks > div',
                         property: 'min-height',
                         measurementFunction: 'innerHeight'
                     },
                     {
                         selector: '.customer-pages-body .equal',
                         property: 'min-height',
                         measurementFunction: 'innerHeight'
                     },
                     {
                         selector: '.cart-footer > div',
                         property: 'min-height',
                         measurementFunction: 'innerHeight'
                     },
                     {
                         selector: '.cart-collaterals > div',
                         property: 'min-height',
                         measurementFunction: 'innerHeight'
                     },
                     {
                         selector: '.post-info > *',
                         property: 'height',
                         measurementFunction: 'innerHeight'
                    },
                    {
                        selector: '.shops-list .shops-item ',
                        property: 'height',
                        measurementFunction: 'innerHeight'
                    }
                ]
            },
            header: {
                modules: [
				    {
				        content: '.header-menu',
				        elementToAttach: '.master-header-wrapper'
				    },
	                {
	                    opener: '.shopping-cart-link > a',
	                    content: '.flyout-cart',
	                    elementToAttach: '.shopping-cart-link',
	                    preventClicking: true,
	                    overlay: false,
	                    scrollbar: flyoutCartScrollbarSelector,
	                    animation: {
	                        type: 'fade '  // Fade, slide, none
	                    }
	                },
	                {
	                    opener: '.personal-button > span',
	                    content: '.header-links-wrapper',
	                    elementToAttach: '.personal-button',
	                    preventClicking: true,
	                    overlay: false,
	                    scrollbar: true,
	                    animation: {
	                        type: 'fade '  // Fade, slide, none
	                    }
	                },
                    {
                        content: '.mega-menu .dropdown',
                        scrollbar: true
                    }
                ]
            },
            stickyNavigation: {
                stickyElement: '.master-header-wrapper',
                stickyElementParent: '.master-header-wrapper-parent',
                showStickyOnFirstReverseScroll: false
            },
            toggle: {
                blocks: [
                     {
                         opener: '.block > .title',
                         content: '.block > .listbox',
                         animation: {
                             type: 'slide'
                         }
                     }
                ]
            },
            responsive: [
                {
	                breakpoint: 1025,
	                settings: {
	                    attachDetach: {
	                        blocks: [
                                {
                                    multipleContentElements: $('.product-variant-list').length,
                                    parentHolder: '.product-variant-line',
                                    traversingFunction: 'find',
                                    content: '.qty-wrapper',
                                    elementToAttach: '.quantity-container'
                                },
                                {
                                    multipleContentElements: $('.blog-posts').length,
                                    parentHolder: '.blog-posts .post',
                                    traversingFunction: 'find',
                                    content: '.post-relations',
                                    elementToAttach: '.blog-details'
                                }
	                        ]
                        },
	                    filters: {
	                        overlayElementSelector: overlayElementSelector,
	                        ajaxFiltersElement: '.nopAjaxFilters7Spikes',
	                        closePanelAfterFiltrationDataAttribute: 'closefilterspanelafterfiltrationinmobile'
	                    },
	                    header: {
	                        modules: [
				                {
				                    opener: '.responsive-nav-wrapper .menu-title > span',
				                    closer: closeMenuSelector,
				                    content: '.header-menu',
				                    elementToAttach: '.master-header-wrapper',
					                overlay: true,
					                scrollbar: true,
					                animation: {
					                    type: 'none' // Fade, slide, none
					                }
				                },
	                            {
	                                opener: '.personal-button > span',
	                                content: '.header-links-wrapper',
	                                elementToAttach: '.personal-button',
	                                preventClicking: true,
	                                overlay: false,
	                                scrollbar: true,
	                                animation: {
	                                    type: 'none' // Fade, slide, none
	                                }
	                            },
				                {
				                    opener: '.filters-button',
				                    closer: '.close-btn',
				                    content: '.nopAjaxFilters7Spikes',
				                    elementToAttach: '.master-wrapper-page',
				                    overlay: {
                                        overlayElementSelector: overlayElementSelector
	                                }
				                },
                                {
                                    content: '.product-sorting',
                                    elementToAttach: '.product-sorting-mobile'
                                },
                                {
                                    content: '.product-page-size',
                                    elementToAttach: '.product-display-mobile'
                                },
                                {
                                    content: '.side-2 .jCarouselMainWrapper',
                                    elementToAttach: '.center-2'
                                }
	                        ]
	                    },
	                    toggle: {
	                        blocks: [
                                 {
                                     opener: '.footer-block > .title',
                                     content: '.footer-block > .list',
                                     animation: {
                                         type: 'slide'
                                     }
                                 },
                                 {
                                     opener: '.block > .title',
                                     content: '.block > .listbox',
                                     animation: {
                                         type: 'slide'
                                     }
                                 }
	                        ]
	                    },
	                    equalizer: {
	                        blocks: [
                                 {
                                     selector: '.spc-categories .item-box',
                                     property: 'height',
                                     measurementFunction: 'innerHeight'
                                 },
                                 {
                                     selector: '.wishlist-content tr',
                                     property: 'height',
                                     measurementFunction: 'innerHeight'
                                 },
                                 {
                                     selector: '.product-list .product-item > div',
                                     property: 'height',
                                     measurementFunction: 'innerHeight'
                                 },
                                 {
                                     selector: '.order-summary-content .cart-item-row',
                                     property: 'height',
                                     measurementFunction: 'innerHeight'
                                 },
                                 {
                                     selector: '.compare-products-mobile .product .item > div',
                                 }
                             ]
	                    },
	                    stickyNavigation: {
	                        stickyElement: '.master-header-wrapper',
	                        stickyElementParent: '.master-header-wrapper-parent',
	                        showStickyOnFirstReverseScroll: true
	                    }
	                }
                }
            ]

        };

        var theme = new sevenSpikesTheme(themeSettings, dependencies, false);

        theme.init();

        handleCustomFileUpload();
        handleLastActiveCheckoutStep();
        handleInboxUnreadPMs();
        handleHomePageCarousel();
        handleSliderTeaserImages();
        setMaxHeightOnMobile('.header-menu');
        setMaxHeightOnMobile('.header-menu .sublist-wrap');

        ssCore.loadImagesOnScroll();

        function handleCustomFileUpload() {

            $('.file-upload input[type="file"]').change(function () {
                if ($(this).parent().find('.filename').length == 0) {
                    $('<input class="filename" type="text" name="filename" value="' + $(this).val().replace(/\\/g, '/').split('/').pop() + '" readonly>').appendTo($(this).parent());   
                } else {
                    $(this).parent().find('input[type=text]').val($(this).val().replace(/\\/g, '/').split('/').pop())
                }
            });
        }

        function handleLastActiveCheckoutStep() {

            // add a class to the last active checkout step. 
            $('.active-step').last().addClass('last');
        }
        
        function handleInboxUnreadPMs() {

            // add a class to the table row for unread messages on Inbox page.
            $('.pm-unread').closest('tr').addClass('unread');
        }

        function handleHomePageCarousel() {
            
            // Create the Left and Right carousel cover elements which will be used like Prev/Next buttons.
            $('.full-width-carousel .home-page-product-grid .slick-carousel').on('init', function () {
                var carouselContainer = $(this);
                slideWidth = carouselContainer.find('.slick-slide').width();
                if ($('.carousel-cover.left').length < 1) {
                    carouselContainer.find('.slick-list').prepend('<a href="javascript:;" Title="Prev" class="carousel-cover left" data-type="prev" style="width:' + slideWidth + 'px"/>');
                }
                if ($('.carousel-cover.right').length < 1) {
                    carouselContainer.find('.slick-list').append('<a href="javascript:;" Title="Next" class="carousel-cover right" data-type="next" style="width:' + slideWidth + 'px"/>');
                }
            });

            var resizeTimer;

            // Change the width of the Prev/Next covers when resizing.
            $(window).resize(function () {
                clearTimeout(resizeTimer);
                resizeTimer = setTimeout(function () {

                    setMaxHeightOnMobile('.header-menu');
                    setMaxHeightOnMobile('.header-menu .sublist-wrap');
                    //wait loading and set surrent width 
                    var carousel = $('.full-width-carousel .home-page-product-grid .slick-carousel'),
                        currentSlideWidth = carousel.find('.slick-slide').attr('style');

                    $('.carousel-cover').attr('style', currentSlideWidth);

                    if ($('.carousel-cover.left').length < 1 && $('.carousel-cover.right').length < 1 && $(this).width() > 1280) {
                        carousel.find('.slick-list').prepend('<a href="javascript:;" Title="Prev" class="carousel-cover left" data-type="prev" style="' + currentSlideWidth + '"/>');
                        carousel.find('.slick-list').append('<a href="javascript:;" Title="Next" class="carousel-cover right" data-type="next" style="' + currentSlideWidth + '"/>');
                    }
                }, 250);
                
            });

            // Trigger click on Prev/Next arrow when clicking on the respective Cover.
            $('body').on('click', '.carousel-cover', function () {
                var carousel = $(this).closest('.nop-jcarousel');

                carousel.find('.slick-' + $(this).attr('data-type')).trigger('click');
            });
        }

        function handleSliderTeaserImages() {

            // Handle Anwywhere sliders teaser images

            $(document).on('nopAnywhereSlidersFinishedLoading', getTeaserImages);
            $(document).on('nivo:animFinished', getTeaserImages);

            function getTeaserImages() {

                var imageLinkSelector = 'a.nivo-imageLink';
                var mainImageSelector = '.nivo-main-image';
                var sliderImagesSelector = 'img:not(' + mainImageSelector + ', .teaserImage)';

                $('.home-page-body .slider-gallery .nivoSlider').each(function () {

                    var hasLinks = $('.nivo-imageLink').length;

                    // Find the main image slider source
                    var mainImageSrc = $(this).find(mainImageSelector).attr('src');

                    // Get all slider images except the main image
                    var sliderImages = $(this).find(' > ' + sliderImagesSelector);

                    if (hasLinks) {
                        
                        sliderImages = $(this).find(' > a ' + sliderImagesSelector);
                    }

                    // Find the corresponding main image from all images
                    var correspondingImage = sliderImages.filter('[src="' + mainImageSrc + '"]');

                    // Select the prev image based on the current one
                    var prevImage = correspondingImage.prev(sliderImagesSelector);

                    // Change the logic for searching the prev image if the slider images have links
                    if (hasLinks) {

                        prevImage = correspondingImage.parent(imageLinkSelector).prev(imageLinkSelector).find(sliderImagesSelector);
                    }

                    // If there is no prev image, just select the last one.
                    if (prevImage.length === 0) {
                        prevImage = sliderImages.last();
                    }

                    // Select the next image based on the current one
                    var nextImage = correspondingImage.next(sliderImagesSelector);

                    // Change the logic for searching the next image if the slider images have links
                    if (hasLinks) {

                        nextImage = correspondingImage.parent(imageLinkSelector).next(imageLinkSelector).find(sliderImagesSelector);
                    }

                    // If there is no next image, just select the first one.
                    if (nextImage.length === 0) {
                        nextImage = sliderImages.first();
                    }

                    if ($('.prevPictureTeaser').length === 0) {

                        // Clone the prev image and place it as a previous picture teaser
                        $('<img />').attr('src', prevImage.attr('src')).fadeIn('fast').insertBefore($(this)).wrap('<div class="prevPictureTeaser"></div>');
                    } else {

                        // If we already have an element, just change the src
                        $('.prevPictureTeaser img').fadeOut('slow', function () {
                            $('.prevPictureTeaser img').attr('src', prevImage.attr('src')).fadeIn('fast');
                        });
                    }

                    if ($('.nextPictureTeaser').length === 0) {

                        // Clone the next image and place it as a next picture teaser
                        $('<img />').attr('src', nextImage.attr('src')).fadeIn('fast').insertBefore($(this)).wrap('<div class="nextPictureTeaser"></div>');
                    } else {

                        // If we already have an element, just change the src
                        $('.nextPictureTeaser img').fadeOut('slow', function () {
                            $('.nextPictureTeaser img').attr('src', nextImage.attr('src')).fadeIn('fast');
                        });
                    }
                });
            }
        }
        //This function is written in a way that allows it to be used in other themes if needed
        function setMaxHeightOnMobile(element, additionalHeight) {

            if (typeof additionalHeight === 'string') {

                additionalHeight = $(additionalHeight).outerHeight();

            } else if (typeof additionalHeight === 'undefined') {

                additionalHeight = 0;

            }

            if ($(window).width() <= 1024) {
                var storeThemeHeight = $('.header-storetheme').length ? $('.header-storetheme').outerHeight() : 0;

                $(element).css('max-height', $(window).height() - additionalHeight - storeThemeHeight);

            }
            else {
                $(element).removeAttr('style');
            }
        }
    });
})(jQuery, sevenSpikesCore);
