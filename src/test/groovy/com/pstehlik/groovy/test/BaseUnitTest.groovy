package com.pstehlik.groovy.test

/**
 * Description missing
 *
 * @author Philip Stehlik
 * @since
 */
abstract class BaseUnitTest
extends GroovyTestCase {
  Map savedMetaClasses

  protected void setUp() {
    super.setUp()
    savedMetaClasses = [:]
  }

  protected void tearDown() {
    super.tearDown()

    // Restore all the saved meta classes.
    savedMetaClasses.each {clazz, metaClass ->
      GroovySystem.metaClassRegistry.removeMetaClass(clazz)
      GroovySystem.metaClassRegistry.setMetaClass(clazz, metaClass)
    }
  }

  /**
   * Use this method when you plan to perform some meta-programming
   * on a class. It ensures that any modifications you make will be
   * cleared at the end of the test.
   * @param clazz The class to register.
   */
  protected void registerMetaClass(Class clazz) {
    // If the class has already been registered, then there's
    // nothing to do.
    if (savedMetaClasses.containsKey(clazz)) return

    // Save the class's current meta class.
    savedMetaClasses[clazz] = clazz.getMetaClass()

    // Create a new EMC for the class and attach it.
    def emc = new ExpandoMetaClass(clazz, true, true)
    emc.initialize()
    GroovySystem.metaClassRegistry.setMetaClass(clazz, emc)
  }

  protected void assertEqualsAndNotNull(String msg, shouldBeThis, checkThis){
    assertEquals msg, shouldBeThis, checkThis
    assertNotNull msg, checkThis
  }

  protected void assertEqualsAndNotNull(shouldBeThis, checkThis){
    assertEquals shouldBeThis, checkThis
    assertNotNull checkThis
  }

  protected void printlnSeparator(String text){
    println "-- ${text} --"
  }
}