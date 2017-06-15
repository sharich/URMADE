<?php get_header(); ?>

<div class="wrap">
	<header class="page-header">
		<h2 class="page-title">Title</h2>
	</header>

	<div id="primary" class="content-area">
		<main id="main" class="site-main" role="main">
		<?php
			if (have_posts()):
				while (have_posts()):
					the_post();
					get_template_part('template-parts/post/content', get_post_format());

				endwhile;

				the_posts_pagination(array(
					'prev_text' => urmade_get_svg(array('icon' => 'arrow-left')) . '<span class="screen-reader-text">' . __('Previous page', 'urmade') . '</span>',
					'next_text' => '<span class="screen-reader-text">' . __('Next page', 'urmade') . '</span>' . urmade_get_svg(array('icon' => 'arrow-right')),
					'before_page_number' => '<span class="meta-nav screen-reader-text">' . __('Page', 'urmade') . ' </span>',
				));

			else:
				get_template_part('template-parts/post/content', 'none');
			endif;
		?>
		</main>
	</div>
</div>

<?php get_footer();
