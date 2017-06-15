<?php get_header(); ?>

<div class="wrap">
	<div id="primary" class="content-area">
		<main id="main" class="site-main" role="main">
		<?php
			if (have_posts()):
				while (have_posts()):
					the_post();
		?>
		<h2 class="page-title"><?php the_title() ?></h2>
		<?php
					the_content();
				endwhile;
			endif;
		?>
		</main>
	</div>
</div>

<?php get_footer();
