tinymce.PluginManager.add('portfoliomedia', function (editor) {
    const open = (type, multiple) => window.documentationEditor?.openMediaPicker(type, multiple);
    const selectFile = (id) => document.getElementById(id)?.click();

    editor.ui.registry.addButton('portfolioimage', {
        text: 'Image',
        tooltip: 'Choisir et insérer une image depuis l’ordinateur',
        onAction: () => selectFile('documentation-image-upload')
    });

    editor.ui.registry.addButton('portfoliogallery', {
        text: 'Galerie',
        tooltip: 'Insérer plusieurs images depuis la médiathèque',
        onAction: () => open('image', true)
    });

    editor.ui.registry.addButton('portfoliovideo', {
        text: 'Vidéo',
        tooltip: 'Choisir et insérer une vidéo depuis l’ordinateur',
        onAction: () => selectFile('documentation-video-upload')
    });

    return { getMetadata: () => ({ name: 'Portfolio media', url: '/admin/media' }) };
});
