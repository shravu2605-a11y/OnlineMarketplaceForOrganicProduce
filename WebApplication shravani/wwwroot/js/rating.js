// Wait for document to load
$(document).ready(function () {

    // Initialize star rating display
    initializeStarRating();

    // Handle star rating hover effect
    $('.rating-stars label').hover(
        function () {
            // Mouse enter - highlight stars
            var starId = $(this).attr('for');
            var starValue = starId.replace('star', '');
            highlightStars(starValue);
        },
        function () {
            // Mouse leave - reset to selected rating
            var selectedRating = $('.rating-stars input:checked').val();
            if (selectedRating) {
                highlightStars(selectedRating);
            } else {
                resetStars();
            }
        }
    );

    // Handle click on stars
    $('.rating-stars label').click(function () {
        var starId = $(this).attr('for');
        var starValue = starId.replace('star', '');

        // Store selected rating in localStorage if needed
        localStorage.setItem('selectedRating', starValue);
    });

    // Load saved rating if any
    var savedRating = localStorage.getItem('selectedRating');
    if (savedRating) {
        highlightStars(savedRating);
    }

    // Function to highlight stars
    function highlightStars(rating) {
        // Reset all stars
        $('.rating-stars label i').removeClass('fas').addClass('far');

        // Highlight selected stars
        for (var i = 1; i <= rating; i++) {
            $('#star' + i + ' + label i').removeClass('far').addClass('fas');
        }
    }

    // Function to reset stars
    function resetStars() {
        $('.rating-stars label i').removeClass('fas').addClass('far');
    }

    // Initialize star rating function
    function initializeStarRating() {
        // For static star displays
        $('.rating i').each(function () {
            // This is handled by server-side rendering
        });
    }

    // Form validation for review
    $('#writeReviewForm').on('submit', function (e) {
        var rating = $('.rating-stars input:checked').val();
        var comment = $('#comment').val().trim();

        if (!rating) {
            e.preventDefault();
            alert('Please select a rating');
            return false;
        }

        if (comment.length < 5) {
            e.preventDefault();
            alert('Please write at least 5 characters for your review');
            return false;
        }

        return true;
    });

    // Auto-resize textarea
    $('textarea').each(function () {
        this.setAttribute('style', 'height:' + (this.scrollHeight) + 'px;overflow-y:hidden');
    }).on('input', function () {
        this.style.height = 'auto';
        this.style.height = (this.scrollHeight) + 'px';
    });

    // Modal cleanup - remove localStorage when modal closes
    $('#writeReviewModal').on('hidden.bs.modal', function () {
        localStorage.removeItem('selectedRating');
        resetStars();
        $('.rating-stars input').prop('checked', false);
    });
});

// Function to load reviews with AJAX (optional)
function loadMoreReviews(productId, page) {
    $.ajax({
        url: '/Product/GetReviews',
        type: 'GET',
        data: { productId: productId, page: page },
        success: function (response) {
            $('#reviewsContainer').append(response);
        },
        error: function (xhr, status, error) {
            console.error('Error loading reviews:', error);
        }
    });
}

// Function to submit review via AJAX (optional)
function submitReview(formData) {
    $.ajax({
        url: '/Product/AddReview',
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        success: function (response) {
            if (response.success) {
                location.reload();
            } else {
                alert(response.message);
            }
        },
        error: function () {
            alert('Error submitting review. Please try again.');
        }
    });
}